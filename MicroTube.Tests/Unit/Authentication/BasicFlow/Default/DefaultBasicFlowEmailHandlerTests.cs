using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication.BasicFlow;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Validation;
using MicroTube.Tests.Mock.Models;
using MicroTube.Tests.Utils;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace MicroTube.Tests.Unit.Authentication.BasicFlow.Default
{
    public class DefaultBasicFlowEmailHandlerTests
    {
		[Fact]
		public async Task ResendEmailConfirmation_Success()
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" }
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			var resendEmailConfirmationResult = await emailHandler.ResendEmailConfirmation(user.Id.ToString());
			Assert.False(resendEmailConfirmationResult.IsError);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().FirstAsync(_ => _.UserId == user.Id);
			Assert.Equal(confirmationStringHash, updatedAuth.EmailConfirmationString);
			Assert.True(DateTime.UtcNow + TimeSpan.FromMinutes(2) > updatedAuth.EmailConfirmationStringExpiration);
			Assert.True(DateTime.UtcNow + TimeSpan.FromSeconds(30) < updatedAuth.EmailConfirmationStringExpiration);
		}
		[Fact]
		public async Task ResendEmailConfirmation_AlreadyConfirmedFail()
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" }
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			var resendEmailConfirmationResult = await emailHandler.ResendEmailConfirmation(user.Id.ToString());
			Assert.True(resendEmailConfirmationResult.IsError);
			Assert.Equal(400, resendEmailConfirmationResult.Code);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().FirstAsync(_ => _.UserId == user.Id);
			Assert.Null(updatedAuth.EmailConfirmationString);
			Assert.Null(updatedAuth.EmailConfirmationStringExpiration);
		}
		[Fact]
		public async Task ResendEmailConfirmation_WrongAuthTypeFail()
		{
			var db = Database.CreateSqliteInMemoryMock();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			var resendEmailConfirmationResult = await emailHandler.ResendEmailConfirmation(user.Id.ToString());
			Assert.True(resendEmailConfirmationResult.IsError);
			Assert.Equal(400, resendEmailConfirmationResult.Code);
			Assert.Equal("Invalid authentication type.", resendEmailConfirmationResult.Error);
		}
		[Fact]
		public async Task ConfirmEmail_Success()
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData 
				{ 
					PasswordHash = "encrypted_password", 
					EmailConfirmationString = confirmationStringHash, 
					EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1)
				}
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			var emailConfirmationResult = await emailHandler.ConfirmEmail(confirmationString);
			Assert.False(emailConfirmationResult.IsError);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_=>_.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.True(updatedAuth.User.IsEqualByContentValues(emailConfirmationResult.GetRequiredObject()));
			Assert.True(updatedAuth.User.IsEmailConfirmed);
			Assert.Null(updatedAuth.EmailConfirmationString);
			Assert.Null(updatedAuth.EmailConfirmationStringExpiration);
		}
		[Theory]
		[InlineData(null, 30)]
		[InlineData("", 30)]
		[InlineData("short", 30)]
		[InlineData("invalid_confirmation_string", 30)]
		[InlineData("confirmation_string", 61)]
		public async Task ConfirmEmail_BadConfirmationStringOrTimeFail(string? requestConfirmationString, int requestTimeOffsetSeconds)
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = false,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password",
					EmailConfirmationString = confirmationStringHash,
					EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(requestTimeOffsetSeconds)
				}
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			secureTokensProvider.HashSecureToken(null).Throws(new FormatException());
			secureTokensProvider.HashSecureToken("").Throws(new FormatException());
			secureTokensProvider.HashSecureToken("short").Throws(new FormatException());
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, "invalid_confirmation_string").Returns(false);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				Substitute.For<IEmailValidator>(),
				Substitute.For<IPasswordEncryption>());
			var emailConfirmationResult = await emailHandler.ConfirmEmail(requestConfirmationString);
			Assert.True(emailConfirmationResult.IsError);
			Assert.Equal(403, emailConfirmationResult.Code);
			Assert.NotNull(emailConfirmationResult.Error);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.False(updatedAuth.User.IsEmailConfirmed);
		}
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ConfirmEmailChange_Success(bool throughConfirmEmail)
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string pendingEmail = "pending@email.com";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password",
					EmailConfirmationString = confirmationStringHash,
					EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1),
					PendingEmail = pendingEmail
				}
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			IServiceResult<AppUser> emailConfirmationResult;
			if (throughConfirmEmail)
				emailConfirmationResult = await emailHandler.ConfirmEmail(confirmationString);
			else
				emailConfirmationResult = await emailHandler.ConfirmEmailChange(confirmationString);
			Assert.False(emailConfirmationResult.IsError);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.True(updatedAuth.User.IsEqualByContentValues(emailConfirmationResult.GetRequiredObject()));
			Assert.True(updatedAuth.User.IsEmailConfirmed);
			Assert.Equal(pendingEmail,updatedAuth.User.Email);
			Assert.Null(updatedAuth.EmailConfirmationString);
			Assert.Null(updatedAuth.EmailConfirmationStringExpiration);
		}
		[Theory]
		[InlineData(null, 30)]
		[InlineData("", 30)]
		[InlineData("short", 30)]
		[InlineData("invalid_confirmation_string", 30)]
		[InlineData("confirmation_string", 61)]
		public async Task ConfirmEmailChange_BadConfirmationStringOrTimeFail(string? requestConfirmationString, int requestTimeOffsetSeconds)
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string pendingEmail = "pending@email.com";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password",
					EmailConfirmationString = confirmationStringHash,
					EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1) - TimeSpan.FromSeconds(requestTimeOffsetSeconds),
					PendingEmail = pendingEmail
				}
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			secureTokensProvider.HashSecureToken(null).Throws(new FormatException());
			secureTokensProvider.HashSecureToken("").Throws(new FormatException());
			secureTokensProvider.HashSecureToken("short").Throws(new FormatException());
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, "invalid_confirmation_string").Returns(false);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				Substitute.For<IEmailValidator>(),
				Substitute.For<IPasswordEncryption>());
			var emailConfirmationResult = await emailHandler.ConfirmEmailChange(requestConfirmationString);
			Assert.True(emailConfirmationResult.IsError);
			Assert.Equal(403, emailConfirmationResult.Code);
			Assert.NotNull(emailConfirmationResult.Error);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.Equal(updatedAuth.User.Email, user.Email);
		}
		[Fact]
		public async Task ConfirmEmailChange_PendingEmailOccupiedFail()
		{
			var db = Database.CreateSqliteInMemory();
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string pendingEmail = "pending@email.com";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password",
					EmailConfirmationString = confirmationStringHash,
					EmailConfirmationStringExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1),
					PendingEmail = pendingEmail
				}
			};
			user.Authentication.User = user;
			var userWithSameEmail = new AppUser
			{
				Email = pendingEmail,
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username1",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password"
				}
			};
			userWithSameEmail.Authentication.User = userWithSameEmail;
			db.AddRange(user, userWithSameEmail);
			db.SaveChanges();
			IBasicFlowEmailHandler emailHandler = CreateTestBasicFlowHandler(db, user, confirmationString, confirmationStringHash);
			IServiceResult<AppUser> emailConfirmationResult = await emailHandler.ConfirmEmail(confirmationString);
			Assert.True(emailConfirmationResult.IsError);
			Assert.Equal(400, emailConfirmationResult.Code);
			Assert.Equal("Email already in use, try another one.", emailConfirmationResult.Error);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.NotEqual(pendingEmail, updatedAuth.User.Email);
		}
		[Fact]
		public async Task StartEmailChange_Success()
		{
			var db = Database.CreateSqliteInMemory();
			string password = "password";
			string encryptedPassword = "encrypted_password";
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string newEmail = "new@email.com";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password"
				}
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			IPasswordEncryption passwordEncryption = Substitute.For<IPasswordEncryption>();
			passwordEncryption.Validate(encryptedPassword, password).Returns(true);
			secureTokensProvider.GenerateSecureToken().Returns(confirmationString);
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, confirmationString).Returns(true);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			authEmailManager.SendEmailConfirmation(user.Email, confirmationString).Returns(Task.CompletedTask);
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				Substitute.For<IEmailValidator>(),
				passwordEncryption);
			var emailConfirmationResult = await emailHandler.StartEmailChange(user.Id.ToString(), newEmail, password);
			Assert.False(emailConfirmationResult.IsError);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.Equal(newEmail, updatedAuth.PendingEmail);
			Assert.Equal(confirmationStringHash, updatedAuth.EmailConfirmationString);
			Assert.True(updatedAuth.EmailConfirmationStringExpiration > DateTime.UtcNow + TimeSpan.FromSeconds(30));
			Assert.True(updatedAuth.EmailConfirmationStringExpiration < DateTime.UtcNow + TimeSpan.FromSeconds(70));
		}
		[Theory]
		[InlineData(null, "new@email.com", "password", true)]
		[InlineData("", "new@email.com", "password", true)]
		[InlineData("non_guid_id", "new@email.com", "password", true)]
		[InlineData("11111111-2ad0-4a2a-b422-ca4ab382e1e2", "new@email.com", "password", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", null, "password", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "", "password", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "invalid_email", "password", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "new@email.com", null, true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "new@email.com", "", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "new@email.com", "invalid_password", true)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "new@email.com", "password", false)]
		[InlineData("2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1", "other_user@email.com", "password", true)]
		public async Task StartEmailChange_BadArgumentsFail(string? requestUserId, string? requestNewEmail, string? requestPassword, bool emailConfirmed)
		{
			var db = Database.CreateSqliteInMemory();
			string password = "password";
			string encryptedPassword = "encrypted_password";
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string userId = "2dcc7e8a-2ad0-4a2a-b422-ca4ab382e1e1";
			var user = new AppUser
			{
				Id = new Guid(userId),
				Email = "email@email.com",
				IsEmailConfirmed = emailConfirmed,
				PublicUsername = "username",
				Username = "username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password"
				}
			};
			var otherUser = new AppUser
			{
				Email = "other_user@email.com",
				IsEmailConfirmed = emailConfirmed,
				PublicUsername = "username",
				Username = "other_username",
				Authentication = new BasicFlowAuthenticationData
				{
					PasswordHash = "encrypted_password"
				}
			};
			otherUser.Authentication.User = otherUser;
			user.Authentication.User = user;
			db.AddRange(user, otherUser);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			IPasswordEncryption passwordEncryption = Substitute.For<IPasswordEncryption>();
			passwordEncryption.Validate(encryptedPassword, password).Returns(true);
			secureTokensProvider.GenerateSecureToken().Returns(confirmationString);
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, confirmationString).Returns(true);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			authEmailManager.SendEmailConfirmation(user.Email, confirmationString).Returns(Task.CompletedTask);
			IEmailValidator emailValidator = Substitute.For<IEmailValidator>();
			emailValidator.Validate("").ReturnsForAnyArgs(ServiceResult.Fail(400, "Bad email"));
			emailValidator.Validate("new@email.com").Returns(ServiceResult.Success());
			emailValidator.Validate("other_user@email.com").Returns(ServiceResult.Success());
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				emailValidator,
				passwordEncryption);
			var emailConfirmationResult = await emailHandler.StartEmailChange(requestUserId, requestNewEmail, requestPassword);
			Assert.True(emailConfirmationResult.IsError);
			Assert.True(emailConfirmationResult.Code == 403 || emailConfirmationResult.Code == 400);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstAsync(_ => _.UserId == user.Id);
			Assert.NotNull(updatedAuth.User);
			Assert.Null(updatedAuth.PendingEmail);
			Assert.Null(updatedAuth.EmailConfirmationString);
			Assert.Null(updatedAuth.EmailConfirmationStringExpiration);
		}
		[Fact]
		public async Task StartEmailChange_WrongAuthTypeFail()
		{
			var db = Database.CreateSqliteInMemoryMock();
			string password = "password";
			string encryptedPassword = "encrypted_password";
			string confirmationString = "confirmation_string";
			string confirmationStringHash = "confirmation_string_hash";
			string newEmail = "new@email.com";
			var user = new AppUser
			{
				Email = "email@email.com",
				IsEmailConfirmed = true,
				PublicUsername = "username",
				Username = "username",
				Authentication = new MockAuthenticationData()
			};
			user.Authentication.User = user;
			db.Add(user);
			db.SaveChanges();
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			IPasswordEncryption passwordEncryption = Substitute.For<IPasswordEncryption>();
			passwordEncryption.Validate(encryptedPassword, password).Returns(true);
			secureTokensProvider.GenerateSecureToken().Returns(confirmationString);
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, confirmationString).Returns(true);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			authEmailManager.SendEmailConfirmation(user.Email, confirmationString).Returns(Task.CompletedTask);
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				Substitute.For<IEmailValidator>(),
				passwordEncryption);
			var emailConfirmationResult = await emailHandler.StartEmailChange(user.Id.ToString(), newEmail, password);
			Assert.True(emailConfirmationResult.IsError);
			Assert.Equal(403, emailConfirmationResult.Code);
			Assert.Equal("Wrong authentication type.", emailConfirmationResult.Error);
			var updatedAuth = await db.AuthenticationData.OfType<BasicFlowAuthenticationData>().Include(_ => _.User).FirstOrDefaultAsync(_ => _.UserId == user.Id);
			Assert.Null(updatedAuth);
		}
		private IBasicFlowEmailHandler CreateTestBasicFlowHandler(MicroTubeDbContext db, AppUser user, string? confirmationString, string? confirmationStringHash)
		{
			var config = new ConfigurationBuilder().AddConfigObject(EmailConfirmationOptions.KEY, new EmailConfirmationOptions(60, 64)).Build();
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			secureTokensProvider.GenerateSecureToken().Returns(confirmationString);
			secureTokensProvider.HashSecureToken(confirmationString).Returns(confirmationStringHash);
			secureTokensProvider.Validate(confirmationStringHash, confirmationString).Returns(true);
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			authEmailManager.SendEmailConfirmation(user.Email, confirmationString).Returns(Task.CompletedTask);
			IBasicFlowEmailHandler emailHandler = new DefaultBasicFlowEmailHandler(
				Substitute.For<ILogger<DefaultBasicFlowEmailHandler>>(),
				db,
				secureTokensProvider,
				config,
				authEmailManager,
				Substitute.For<IEmailValidator>(),
				Substitute.For<IPasswordEncryption>());
			return emailHandler;
		}
	}
}
