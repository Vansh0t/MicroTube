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
        [HttpGet("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ConfirmEmail(string emailConfirmationString)
        {
            var resultJWT = await _emailPasswordAuthentication.ConfirmEmail(emailConfirmationString);
            if (resultJWT.IsError || resultJWT.ResultObject == null)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok(new AuthenticationResponseDTO(resultJWT.ResultObject));
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
        [HttpGet("ChangeEmailConfirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeEmailEnd(string emailChangeConfirmationString)
        {
            var resultJWT = await _emailPasswordAuthentication.ConfirmEmailChange(emailChangeConfirmationString);
            if (resultJWT.IsError)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok("An email will be sent to this email if an account is registered under it.");
        }
        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPasswordStart(ResetPasswordStartDTO resetData)
        {
            var result = await _emailPasswordAuthentication.StartPasswordReset(resetData.Email);
            if(result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok("An email will be sent to this email if an account is registered under it.");
        }
        [HttpPost("ValidatePasswordReset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ValidatePasswordReset([FromBody] string passwordResetString)
        {
            var result = await _emailPasswordAuthentication.UsePasswordResetString(passwordResetString);
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
            var result = await  _emailPasswordAuthentication.ChangePassword(userId, changeData.NewPassword, changeData.ConfirmedNewPassword);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok();
        }
    }
}