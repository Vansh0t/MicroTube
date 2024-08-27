using MicroTube.Services.Cryptography;

namespace MicroTube.Tests.Unit.Cryptography
{
	public class PBKDF2PasswordEncryptionTests
	{
		[Fact]
		public void Encrypt_Success()
		{
			IPasswordEncryption passwordEncryption = new PBKDF2PasswordEncryption();
			string encryptedPassword1 = passwordEncryption.Encrypt("password_to_encrypt");
			string encryptedPassword2 = passwordEncryption.Encrypt("password_to_encrypt");
			Assert.NotEmpty(encryptedPassword1);
			Assert.NotEmpty(encryptedPassword2);
			Assert.NotEqual("password_to_encrypt", encryptedPassword1);
			Assert.NotEqual("password_to_encrypt", encryptedPassword2);
			Assert.NotEqual(encryptedPassword1, encryptedPassword2);
		}
		[Fact]
		public void Validate_Success()
		{
			string password = "password_to_encrypt";
			IPasswordEncryption passwordEncryption = new PBKDF2PasswordEncryption();
			string encryptedPassword = passwordEncryption.Encrypt(password);
			Assert.True(passwordEncryption.Validate(encryptedPassword, password));
			Assert.False(passwordEncryption.Validate(encryptedPassword, "password_to_encryptt"));
		}
	}
}
