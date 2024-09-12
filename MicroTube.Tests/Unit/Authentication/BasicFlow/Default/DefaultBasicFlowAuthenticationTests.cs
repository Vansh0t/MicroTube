using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication.BasicFlow;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Validation;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;
using NSubstitute;
using System.Reflection.Metadata;

namespace MicroTube.Tests.Unit.Authentication.BasicFlow.Default
{

    public class DefaultBasicFlowAuthenticationTests
    {
        [Fact]
        public async Task TestCreateUser_Success()
        {
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string confirmationString = "blablastring";

			using var db = Database.CreateSqliteInMemory();
			IUsernameValidator mockUsernameValidator = Substitute.For<IUsernameValidator>();
			mockUsernameValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IEmailValidator mockEmailValidator = Substitute.For<IEmailValidator>();
			mockEmailValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IPasswordValidator mockPasswordValidator = Substitute.For<IPasswordValidator>();
			mockPasswordValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IPasswordEncryption mockPasswordEncryption = Substitute.For<IPasswordEncryption>();
			mockPasswordEncryption.Encrypt("password").Returns("encrypted_password");
			IBasicFlowEmailHandler mockEmailHandler = Substitute.For<IBasicFlowEmailHandler>();
			BasicFlowAuthenticationData mockAuthData = new() { PasswordHash = "encrypted_password" };
			var expiration = DateTime.UtcNow;
			mockEmailHandler.ApplyEmailConfirmation(mockAuthData, out Arg.Any<string>()).ReturnsForAnyArgs(call =>
			{
				var authData = call.ArgAt<BasicFlowAuthenticationData>(0);
				authData.EmailConfirmationString = confirmationString;
				authData.EmailConfirmationStringExpiration = expiration;
				call[1] = confirmationString;
				return authData;
			});
			IAuthenticationEmailManager emailManager = Substitute.For<IAuthenticationEmailManager>();
			emailManager.SendEmailConfirmation(email, confirmationString).Returns(Task.CompletedTask);
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(mockUsernameValidator,
																					  mockEmailValidator,
																					  mockPasswordValidator,
																					  mockPasswordEncryption,
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, mockEmailHandler, emailManager);
			var authResult = await auth.CreateUser(username, email, password);
			Assert.False(authResult.IsError);
			var user = authResult.GetRequiredObject();
			var userFromDb = db.Users.Include(_=>_.Authentication).FirstOrDefault(_ => _.Id == user.Id);
			Assert.NotNull(user);
			Assert.NotNull(userFromDb);
			Assert.True(user.IsEqualByContentValues(userFromDb));
			Assert.NotNull(userFromDb.Authentication);
			Assert.True(userFromDb.Authentication is BasicFlowAuthenticationData);
			var authFromDb = (BasicFlowAuthenticationData)userFromDb.Authentication;
			Assert.Equal(confirmationString, authFromDb.EmailConfirmationString);
			Assert.Equal(expiration, authFromDb.EmailConfirmationStringExpiration);
			Assert.False(userFromDb.IsEmailConfirmed);
        }
		[Fact]
		public async Task TestCreateUser_ValidationFail()
		{
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string usernameError = "Bad username";
			string emailError = "Bad email";
			string passwordError = "Bad password";

			using var db = Database.CreateSqliteInMemory();
			var success = ServiceResult.Success();
			IUsernameValidator mockUsernameValidator = Substitute.For<IUsernameValidator>();
			mockUsernameValidator.Validate(username).ReturnsForAnyArgs(ServiceResult.Fail(400, usernameError), success, success, ServiceResult.Fail(400, usernameError));
			IEmailValidator mockEmailValidator = Substitute.For<IEmailValidator>();
			mockEmailValidator.Validate(email).ReturnsForAnyArgs(success, ServiceResult.Fail(400, emailError), ServiceResult.Success(), ServiceResult.Fail(400, emailError));
			IPasswordValidator mockPasswordValidator = Substitute.For<IPasswordValidator>();
			mockPasswordValidator.Validate(password).ReturnsForAnyArgs(success, success, ServiceResult.Fail(400, passwordError), ServiceResult.Fail(400, passwordError));
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(mockUsernameValidator,
																					  mockEmailValidator,
																					  mockPasswordValidator,
																					  Substitute.For<IPasswordEncryption>(),
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, Substitute.For<IBasicFlowEmailHandler>(), Substitute.For<IAuthenticationEmailManager>());
			var authResult = await auth.CreateUser(username, email, password);
			Assert.True(authResult.IsError);
			Assert.Contains(usernameError, authResult.Error);
			Assert.Equal(400, authResult.Code);
			authResult = await auth.CreateUser(username, email, password);
			Assert.True(authResult.IsError);
			Assert.Contains(emailError, authResult.Error);
			Assert.Equal(400, authResult.Code);
			authResult = await auth.CreateUser(username, email, password);
			Assert.True(authResult.IsError);
			Assert.Contains(passwordError, authResult.Error);
			Assert.Equal(400, authResult.Code);
			authResult = await auth.CreateUser(username, email, password);
			Assert.True(authResult.IsError);
			Assert.Contains(usernameError, authResult.Error);
			Assert.Contains(emailError, authResult.Error);
			Assert.Contains(passwordError, authResult.Error);
			Assert.Equal(400, authResult.Code);
		}
		[Theory]
		[InlineData("username", null)]
		[InlineData(null, "email@email.com")]
		[InlineData("username", "email@email.com")]
		public async Task TestCreateUser_UsernameOrEmailOccupiedFail( string? existingUserUsername, string? existingUserEmail)
		{
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string confirmationString = "blablastring";
			//using var db = Database.CreateSqlite("/");
			using var db = Database.CreateSqliteInMemory();
			var createdUser = new AppUser
			{
				Username = existingUserUsername != null? existingUserUsername : "username2",
				PublicUsername = username,
				Email = existingUserEmail != null? existingUserEmail: "email2@email.com",
				IsEmailConfirmed = false,
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" }
			};
			createdUser.Authentication.User = createdUser;
			db.Add(createdUser);
			db.SaveChanges();
			IUsernameValidator mockUsernameValidator = Substitute.For<IUsernameValidator>();
			mockUsernameValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IEmailValidator mockEmailValidator = Substitute.For<IEmailValidator>();
			mockEmailValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IPasswordValidator mockPasswordValidator = Substitute.For<IPasswordValidator>();
			mockPasswordValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Success());
			IPasswordEncryption mockPasswordEncryption = Substitute.For<IPasswordEncryption>();
			mockPasswordEncryption.Encrypt("password").Returns("encrypted_password");
			IBasicFlowEmailHandler mockEmailHandler = Substitute.For<IBasicFlowEmailHandler>();
			BasicFlowAuthenticationData mockAuthData = new() { PasswordHash = "encrypted_password" };
			var expiration = DateTime.UtcNow;
			mockEmailHandler.ApplyEmailConfirmation(mockAuthData, out Arg.Any<string>()).ReturnsForAnyArgs(call =>
			{
				var authData = call.ArgAt<BasicFlowAuthenticationData>(0);
				authData.EmailConfirmationString = confirmationString;
				authData.EmailConfirmationStringExpiration = expiration;
				call[1] = confirmationString;
				return authData;
			});
			IAuthenticationEmailManager emailManager = Substitute.For<IAuthenticationEmailManager>();
			emailManager.SendEmailConfirmation(email, confirmationString).Returns(Task.CompletedTask);
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(mockUsernameValidator,
																					  mockEmailValidator,
																					  mockPasswordValidator,
																					  mockPasswordEncryption,
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, mockEmailHandler, emailManager);
			var authResult = await auth.CreateUser(username, email, password);
			var users = await db.Users.ToArrayAsync();
			Assert.True(authResult.IsError);
			Assert.Equal(400, authResult.Code);
			Assert.Equal("Username or Email are already in use.", authResult.Error);
		}
		[Fact]
		public async Task TestSighIn_Success()
		{
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string passwordHash = "encrypted_password";

			using var db = Database.CreateSqliteInMemory();
			var createdUser = new AppUser
			{
				Username = username,
				PublicUsername = username,
				Email = email,
				IsEmailConfirmed = false
			};
			createdUser.Authentication = new BasicFlowAuthenticationData { PasswordHash = passwordHash, User = createdUser };
			db.Add(createdUser);
			db.SaveChanges();
			IPasswordEncryption mockPasswordEncryption = Substitute.For<IPasswordEncryption>();
			mockPasswordEncryption.Validate(passwordHash, password).Returns(true);
			IBasicFlowEmailHandler mockEmailHandler = Substitute.For<IBasicFlowEmailHandler>();
			IAuthenticationEmailManager emailManager = Substitute.For<IAuthenticationEmailManager>();
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(Substitute.For<IUsernameValidator>(),
																					  Substitute.For<IEmailValidator>(),
																					  Substitute.For<IPasswordValidator>(),
																					  mockPasswordEncryption,
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, mockEmailHandler, emailManager);
			var authResultUsername = await auth.SignIn(username, password);
			Assert.False(authResultUsername.IsError);
			var user = authResultUsername.GetRequiredObject();
			var userFromDb = db.Users.FirstOrDefault(_ => _.Id == user.Id);
			Assert.NotNull(user);
			Assert.NotNull(userFromDb);
			Assert.True(user.IsEqualByContentValues(userFromDb));
			var authResultEmail = await auth.SignIn(email, password);
			Assert.False(authResultEmail.IsError);
			user = authResultEmail.GetRequiredObject();
			Assert.NotNull(user);
			Assert.NotNull(userFromDb);
			Assert.True(user.IsEqualByContentValues(userFromDb));
		}
		[Theory]
		[InlineData("username", "", 400)]
		[InlineData("username", null, 400)]
		[InlineData("username", "wrong_pwd", 401)]
		[InlineData("wrong_username", "password", 401)]
		[InlineData("", "password", 400)]
		[InlineData(null, "password", 400)]
		public async Task TestSighIn_BadCredentialsFail(string? loginCredential, string? loginPassword, int expectedStatusCode)
		{
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string passwordHash = "encrypted_password";

			using var db = Database.CreateSqliteInMemory();
			var createdUser = new AppUser
			{
				Username = username,
				PublicUsername = username,
				Email = email,
				IsEmailConfirmed = false
			};
			createdUser.Authentication = new BasicFlowAuthenticationData { PasswordHash = passwordHash, User = createdUser };
			db.Add(createdUser);
			db.SaveChanges();
			IPasswordEncryption mockPasswordEncryption = Substitute.For<IPasswordEncryption>();
			mockPasswordEncryption.Validate(passwordHash, password).Returns(true);
			mockPasswordEncryption.Validate(passwordHash, "wrong_pwd").Returns(false);
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(Substitute.For<IUsernameValidator>(),
																					  Substitute.For<IEmailValidator>(),
																					  Substitute.For<IPasswordValidator>(),
																					  mockPasswordEncryption,
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, Substitute.For<IBasicFlowEmailHandler>(), Substitute.For<IAuthenticationEmailManager>());
			var authResult = await auth.SignIn(loginCredential!, loginPassword!);
			Assert.True(authResult.IsError);
			Assert.Equal(expectedStatusCode, authResult.Code);
			Assert.NotNull(authResult.Error);
		}
		[Fact]
		public async Task TestSighIn_WrongAuthenticationTypeFail()
		{
			string username = "username";
			string email = "email@email.com";
			string password = "password";
			string passwordHash = "encrypted_password";

			using var db = Database.CreateSqliteInMemoryMock();
			var createdUser = new AppUser
			{
				Username = username,
				PublicUsername = username,
				Email = email,
				IsEmailConfirmed = false
			};
			createdUser.Authentication = new MockAuthenticationData { User = createdUser };
			db.Add(createdUser);
			db.SaveChanges();
			IPasswordEncryption mockPasswordEncryption = Substitute.For<IPasswordEncryption>();
			mockPasswordEncryption.Validate(passwordHash, password).Returns(true);
			mockPasswordEncryption.Validate(passwordHash, "wrong_pwd").Returns(false);
			IBasicFlowAuthenticationProvider auth = new DefaultBasicFlowAuthenticationProvider(Substitute.For<IUsernameValidator>(),
																					  Substitute.For<IEmailValidator>(),
																					  Substitute.For<IPasswordValidator>(),
																					  mockPasswordEncryption,
																					  Substitute.For<ILogger<DefaultBasicFlowAuthenticationProvider>>(),
																					  db, Substitute.For<IBasicFlowEmailHandler>(), Substitute.For<IAuthenticationEmailManager>());
			var authResult = await auth.SignIn(username, password);
			Assert.True(authResult.IsError);
			Assert.Equal(401, authResult.Code);
			Assert.NotNull(authResult.Error);

		}
	}
}
