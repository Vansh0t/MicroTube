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

namespace MicroTube.Tests.Unit.Authentication.BasicFlow.Default
{
    public class DefaultBasicFlowPasswordHandlerTests
    {
		[Fact]
		public async Task StartPasswordReset_Success()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string passwordResetStringHash = "password_reset_string_hash";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" };
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				passwordResetStringHash,
				"new_password",
				jwtPasswordResetToken,
				authData.PasswordHash,
				"encrypted_new_password");
			var result = await passwordHandler.StartPasswordReset(user.Email);
			Assert.False(result.IsError);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.NotNull(updatedAuthData.PasswordResetString);
			Assert.NotNull(updatedAuthData.PasswordResetStringExpiration);
			Assert.True(updatedAuthData.PasswordResetStringExpiration > DateTime.UtcNow + TimeSpan.FromSeconds(30));
			Assert.True(updatedAuthData.PasswordResetStringExpiration < DateTime.UtcNow + TimeSpan.FromSeconds(70));
		}
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("wrongemail.com")]
		public async Task StartPasswordReset_InvalidEmailFail(string? requestEmail)
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string passwordResetStringHash = "password_reset_string_hash";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" };
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				passwordResetStringHash,
				"new_password",
				jwtPasswordResetToken,
				authData.PasswordHash,
				"encrypted_new_password");
			var result = await passwordHandler.StartPasswordReset(requestEmail);
			Assert.True(result.IsError);
			Assert.True(result.Code == 400 || result.Code == 403);
			Assert.NotNull(result.Error);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.Null(updatedAuthData.PasswordResetString);
			Assert.Null(updatedAuthData.PasswordResetStringExpiration);
		}
		[Fact]
		public async Task StartPasswordReset_NoUserFoundFail()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordHash = "encrypted_password";
			string passwordResetString = "password_reset_string";
			string passwordResetStringHash = "password_reset_string_hash";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			string email = "email@email.com";
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				Guid.NewGuid().ToString(),
				password,
				email,
				passwordResetString,
				passwordResetStringHash,
				"new_password",
				jwtPasswordResetToken,
				passwordHash,
				"encrypted_new_password");
			var result = await passwordHandler.StartPasswordReset(email);
			Assert.False(result.IsError); //user does not exist, resulting in failure, but expect success response due to security standards
		}
		[Fact]
		public async Task StartPasswordReset_UserEmailNotConfirmedFail()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string passwordResetStringHash = "password_reset_string_hash";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = false
			};
			var authData = new BasicFlowAuthenticationData { PasswordHash = "encrypted_password" };
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				passwordResetStringHash,
				"new_password",
				jwtPasswordResetToken,
				authData.PasswordHash,
				"encrypted_new_password");
			var result = await passwordHandler.StartPasswordReset(user.Email);
			Assert.False(result.IsError); //user does not have email confirmed, resulting in failure, but expect success response due to security standards
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.Null(updatedAuthData.PasswordResetString);
			Assert.Null(updatedAuthData.PasswordResetStringExpiration);
		}
		[Fact]
		public async Task StartPasswordReset_WrongAuthTypeFail()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string passwordResetStringHash = "password_reset_string_hash";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new MockAuthenticationData ();
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				passwordResetStringHash,
				"new_password",
				jwtPasswordResetToken,
				"encrypted_password",
				"encrypted_new_password");
			var result = await passwordHandler.StartPasswordReset(user.Email);
			Assert.False(result.IsError); //user does not have email confirmed, resulting in failure, but expect success response due to security standards
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().FirstOrDefault(_ => _.PasswordResetString == passwordResetStringHash);
			Assert.Null(updatedAuthData);
		}
		[Fact]
		public async Task UsePasswordResetString_Success()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData 
			{ 
				PasswordHash = "encrypted_password",
				PasswordResetString = "password_reset_string_hash",
				PasswordResetStringExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(60)
			};
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				authData.PasswordResetString,
				"new_password",
				jwtPasswordResetToken,
				authData.PasswordHash,
				"encrypted_new_password");
			var result = await passwordHandler.UsePasswordResetString(passwordResetString);
			Assert.False(result.IsError);
			Assert.Equal(jwtPasswordResetToken, result.GetRequiredObject());
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.Null(updatedAuthData.PasswordResetString);
			Assert.Null(updatedAuthData.PasswordResetStringExpiration);
		}
		[Theory]
		[InlineData(null, 0)]
		[InlineData("", 0)]
		[InlineData("invalid_reset_string", 0)]
		[InlineData("password_reset_string", 70)]
		public async Task UsePasswordResetString_InvalidResetStringFail(string? requestResetString, int requestTimeOffsetSeconds)
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData
			{
				PasswordHash = "encrypted_password",
				PasswordResetString = "password_reset_string_hash",
				PasswordResetStringExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(60) - TimeSpan.FromSeconds(requestTimeOffsetSeconds)
			};
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				authData.PasswordResetString,
				"new_password",
				jwtPasswordResetToken,
				authData.PasswordHash, 
				"encrypted_new_password");
			var result = await passwordHandler.UsePasswordResetString(requestResetString);
			Assert.True(result.IsError);
			Assert.Null(result.ResultObject);
			Assert.Equal(403, result.Code);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.NotNull(updatedAuthData.PasswordResetString);
			Assert.NotNull(updatedAuthData.PasswordResetStringExpiration);
		}
		[Fact]
		public async Task ChangePassword_Success()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			string newPassword = "new_password";
			string newPasswordHash = "encrypted_new_password";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData
			{
				PasswordHash = "encrypted_password"
			};
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				"password_reset_string_hash",
				newPassword,
				jwtPasswordResetToken,
				authData.PasswordHash,
				newPasswordHash);
			var result = await passwordHandler.ChangePassword(user.Id.ToString(), newPassword);
			Assert.False(result.IsError);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.Null(updatedAuthData.PasswordResetString);
			Assert.Null(updatedAuthData.PasswordResetStringExpiration);
			Assert.Equal(newPasswordHash, updatedAuthData.PasswordHash);
		}
		[Theory]
		[InlineData(null, "new_password")]
		[InlineData("", "new_password")]
		[InlineData("no_guild_id", "new_password")]
		[InlineData("47b6697a-1111-1111-990b-1caf155cb708", "new_password")]
		[InlineData("47b6697a-b56b-49a4-990b-1caf155cb708", null)]
		[InlineData("47b6697a-b56b-49a4-990b-1caf155cb708", " ")]
		[InlineData("47b6697a-b56b-49a4-990b-1caf155cb708", "invalid_new_password")]
		public async Task ChangePassword_InvalidNewPasswordOrUserIdFail(string? userId, string? newPassword)
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemory();
			string password = "password";
			string passwordResetString = "password_reset_string";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			string newPasswordHash = "encrypted_new_password";
			string validUserId = "47b6697a-b56b-49a4-990b-1caf155cb708";
			string validNewPassword = "new_password";
			var user = new AppUser
			{
				Id = new Guid(validUserId),
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new BasicFlowAuthenticationData
			{
				PasswordHash = "encrypted_password",
				PasswordResetString = "password_reset_string_hash",
				PasswordResetStringExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(60)
			};
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				authData.PasswordResetString,
				validNewPassword,
				jwtPasswordResetToken,
				authData.PasswordHash,
				newPasswordHash);
			var result = await passwordHandler.ChangePassword(userId, newPassword);
			Assert.True(result.IsError);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().First(_ => authData.Id == _.Id);
			Assert.NotEqual(newPasswordHash, updatedAuthData.PasswordHash);
		}
		[Fact]
		public async Task ChangePassword_WrongAuthTypeFail()
		{
			using MicroTubeDbContext db = Database.CreateSqliteInMemoryMock();
			string password = "password";
			string passwordHash = "password_hash";
			string passwordResetString = "password_reset_string";
			string jwtPasswordResetToken = "jwt_pwd_reset_token";
			string newPassword = "new_password";
			string newPasswordHash = "encrypted_new_password";
			var user = new AppUser
			{
				Email = "email@email.com",
				Username = "username",
				PublicUsername = "username",
				IsEmailConfirmed = true
			};
			var authData = new MockAuthenticationData();
			user.Authentication = authData;
			authData.User = user;
			db.Add(user);
			db.SaveChanges();
			IBasicFlowPasswordHandler passwordHandler = CreatePasswordHandler(
				db,
				user.Id.ToString(),
				password,
				user.Email,
				passwordResetString,
				"password_reset_string_hash",
				newPassword,
				jwtPasswordResetToken,
				passwordHash,
				newPasswordHash);
			var result = await passwordHandler.ChangePassword(user.Id.ToString(), newPassword);
			Assert.True(result.IsError);
			Assert.Equal(403, result.Code);
			var updatedAuthData = db.AuthenticationData.OfType<BasicFlowAuthenticationData>().FirstOrDefault(_ => authData.Id == _.Id);
			Assert.Null(updatedAuthData);
		}
		private IBasicFlowPasswordHandler CreatePasswordHandler(
			MicroTubeDbContext db,
			string validUserId,
			string validPassword,
			string validEmail,
			string validPasswordResetString,
			string validPasswordResetStringHash,
			string validNewPassword,
			string jwtPasswordResetToken,
			string encryptedPassword,
			string encryptedNewPassword)
		{
			IEmailValidator emailValidator = Substitute.For<IEmailValidator>();
			emailValidator.Validate(validEmail).Returns(ServiceResult.Success());
			emailValidator.Validate(Arg.Is<string>(_=>_ != validEmail)).Returns(ServiceResult.Fail(400, "Invalid email"));
			IAuthenticationEmailManager authEmailManager = Substitute.For<IAuthenticationEmailManager>();
			authEmailManager.SendPasswordResetStart(validEmail, validPasswordResetString).Returns(Task.CompletedTask);
			authEmailManager.SendPasswordResetStart(Arg.Is<string>(_ => _ != validEmail), Arg.Is<string>(_ => _ != validPasswordResetString))
				.Throws(new Exception("Emailing exception"));
			ISecureTokensProvider secureTokensProvider = Substitute.For<ISecureTokensProvider>();
			secureTokensProvider.GenerateSecureToken().Returns(validPasswordResetString);
			secureTokensProvider.HashSecureToken(validPasswordResetString).Returns(validPasswordResetStringHash);
			secureTokensProvider.HashSecureToken(Arg.Is<string>(_ => _ != validPasswordResetString)).Throws(new FormatException("Invalid token format"));
			secureTokensProvider.Validate(validPasswordResetStringHash, validPasswordResetString).Returns(true);
			secureTokensProvider.Validate(Arg.Is<string>(_ => _ != validPasswordResetStringHash), Arg.Is<string>(_ => _ != validPasswordResetString)).Returns(false);
			IJwtPasswordResetTokenProvider jwtPasswordResetTokenProvider = Substitute.For<IJwtPasswordResetTokenProvider>();
			jwtPasswordResetTokenProvider.GetToken(validUserId).Returns(ServiceResult<string>.Success(jwtPasswordResetToken));
			jwtPasswordResetTokenProvider.GetToken(Arg.Is<string>(_ => _ != validUserId)).Returns(ServiceResult<string>.FailInternal());
			IPasswordValidator passwordValidator = Substitute.For<IPasswordValidator>();
			passwordValidator.Validate(validPassword).Returns(ServiceResult.Success());
			passwordValidator.Validate(validNewPassword).Returns(ServiceResult.Success());
			passwordValidator.Validate(Arg.Is<string>(_ => _ != validPassword && _ != validNewPassword)).Returns(ServiceResult.Fail(400, "Invalid password"));
			IPasswordEncryption passwordEncryption = Substitute.For<IPasswordEncryption>();
			passwordEncryption.Encrypt(validPassword).Returns(encryptedPassword);
			passwordEncryption.Encrypt(validNewPassword).Returns(encryptedNewPassword);
			passwordEncryption.Encrypt(Arg.Is<string>(_ => _ != validPassword && _ != validNewPassword)).Throws(new Exception("Encryption exception"));
			passwordEncryption.Validate(encryptedPassword, validPassword).Returns(true);
			passwordEncryption.Validate(Arg.Is<string>(_ => _ != encryptedPassword), Arg.Is<string>(_ => _ != validPassword)).Returns(false);
			var config = new ConfigurationBuilder()
				.AddConfigObject(PasswordConfirmationOptions.KEY, new PasswordConfirmationOptions(60, 64))
				.Build();
			return new DefaultBasicFlowPasswordHandler(
				Substitute.For<ILogger<DefaultBasicFlowPasswordHandler>>(),
				emailValidator,
				db,
				authEmailManager,
				secureTokensProvider,
				jwtPasswordResetTokenProvider,
				passwordValidator,
				passwordEncryption,
				config
				);
		}
    }
}
