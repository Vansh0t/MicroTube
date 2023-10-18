using MicroTube.Services;
using MicroTube.Services.Validation;

namespace MicroTube.Tests.Unit.Validation
{
    public class CredentialsValidation
    {
        private readonly IEmailValidator _emailValidator;

        private readonly IPasswordValidator _passwordValidator;

        private readonly IUsernameValidator _usernameValidator;

        public CredentialsValidation()
        {
            _emailValidator = new EmailValidator();
            _passwordValidator = new DefaultPasswordValidator();
            _usernameValidator = new UsernameValidator();
        }

        [Theory]
        [InlineData("wrong")]
        [InlineData("25@**")]
        [InlineData(null)]
        public void TestEmailFail(string email)
        {
            IServiceResult result = _emailValidator.Validate(email);
            Assert.True(result.IsError);
        }
        [Fact]
        public void TestEmailSuccess()
        {
            IServiceResult result = _emailValidator.Validate("test@email.com");
            Assert.False(result.IsError);
        }
        [Theory]
        [InlineData("shor5")]//too short
        [InlineData("longggggggggggggggggggggggggggggggg525")]//too long
        [InlineData("hasnodigits")]//has no digits
        [InlineData("123456789")]//has no letters
        [InlineData(null)]//null
        [InlineData(" ")]//empty space
        [InlineData("")]//empty
        public void TestPasswordFail(string password)
        {
            IServiceResult result = _passwordValidator.Validate(password);
            Assert.True(result.IsError);
        }
        [Fact]
        public void TestPasswordSuccess()
        {
            IServiceResult result = _passwordValidator.Validate("validpwd111");
            Assert.False(result.IsError);
        }
        [Theory]
        [InlineData("15")]//no letters
        [InlineData("s")]//too short
        [InlineData("longgggggggggggggggggggggggggggggggggg")]//too long
        [InlineData(null)]//null
        [InlineData(" ")]//empty space
        [InlineData("")]//empty
        [InlineData("**AA2!")]//has invalid characters
        public void TestUsernameFail(string username)
        {
            IServiceResult result = _usernameValidator.Validate(username);
            Assert.True(result.IsError);
        }
        [Fact]
        public void TestUsernameSuccess()
        {
            IServiceResult result = _usernameValidator.Validate("V4");
            Assert.False(result.IsError);
        }
    }
}
