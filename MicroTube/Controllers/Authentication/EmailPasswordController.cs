using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Constants;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Authentication;
using MicroTube.Services.Authentication.Providers;

namespace MicroTube.Controllers.Authentication
{
    [ApiController]
    [Route("authentication/[controller]")]
    public class EmailPasswordController : ControllerBase
    {
        private readonly ILogger<EmailPasswordController> _logger;
        private readonly EmailPasswordAuthenticationProvider _emailPasswordAuthentication;
        private readonly IJwtClaims _claims;

        public EmailPasswordController(ILogger<EmailPasswordController> logger, EmailPasswordAuthenticationProvider emailPasswordAuthentication, IJwtClaims claims)
        {
            _logger = logger;
            _emailPasswordAuthentication = emailPasswordAuthentication;
            _claims = claims;
        }

        [HttpPost("SignUp")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> SignUp(SignUpEmailPasswordDTO data)
        {
            var resultUser = await _emailPasswordAuthentication.CreateUser(data.Username, data.Email, data.Password);
            var user = resultUser.ResultObject;
            if (resultUser.IsError || user == null)
                return StatusCode(resultUser.Code, resultUser.Error);
            var resultJWT = await _emailPasswordAuthentication.SignIn(user.Email, data.Password);
            if (resultJWT.IsError || resultJWT.ResultObject == null)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok(new AuthenticationResponseDTO(resultJWT.ResultObject));
        }
        [HttpPost("SignIn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> SignIn(SignInCredentialPasswordDTO data)
        {
            var resultJWT = await _emailPasswordAuthentication.SignIn(data.Credential, data.Password);
            if (resultJWT.IsError || resultJWT.ResultObject == null)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok(new AuthenticationResponseDTO(resultJWT.ResultObject));
        }
		[Authorize]
		[HttpPost("ConfirmEmailResend")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ConfirmEmail()
		{
			int userId = _claims.GetUserId(User);
			var resultJWT = await _emailPasswordAuthentication.ResendEmailConfirmation(userId);
			if (resultJWT.IsError)
				return StatusCode(resultJWT.Code, resultJWT.Error);
			return Ok();
		}
		[HttpPost("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ConfirmEmail(MessageDTO emailConfirmationString)
        {
            var resultJWT = await _emailPasswordAuthentication.ConfirmEmail(emailConfirmationString.Message);
            if (resultJWT.IsError || resultJWT.ResultObject == null)
                return StatusCode(resultJWT.Code, resultJWT.Error);
			if(User.Identity != null && User.Identity.IsAuthenticated)
				return Ok(new AuthenticationResponseDTO(resultJWT.ResultObject));
			return Ok();
        }
        [Authorize]
        [HttpPost("ChangeEmailStart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeEmailStart(EmailChangeDTO emailChangeData)
        {
            var userId = _claims.GetUserId(HttpContext.User);
            var resultJWT = await _emailPasswordAuthentication.StartEmailChange(userId, emailChangeData.NewEmail, emailChangeData.Password);
            if (resultJWT.IsError)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok();
        }
        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageDTO))]
        public async Task<IActionResult> ResetPasswordStart(ResetPasswordStartDTO resetData)
        {
            var result = await _emailPasswordAuthentication.StartPasswordReset(resetData.Email);
            if(result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok(new MessageDTO("An email will be sent to this address if an account is registered under it."));
        }
        [HttpPost("ValidatePasswordReset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ValidatePasswordReset(MessageDTO passwordResetString)
        {
            var result = await _emailPasswordAuthentication.UsePasswordResetString(passwordResetString.Message);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            if (result.ResultObject == null)
                throw new RequiredObjectNotFoundException("Result was a success, but ResultObject is null");
            return Ok(new AuthenticationResponseDTO(result.ResultObject));
        }
        [Authorize(AuthorizationConstants.PASSWORD_RESET_ONLY_POLICY)]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ChangePassword(PasswordChangeDTO changeData)
        {
            int userId = _claims.GetUserId(HttpContext.User);
            var result = await  _emailPasswordAuthentication.ChangePassword(userId, changeData.NewPassword);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok();
        }
    }
}