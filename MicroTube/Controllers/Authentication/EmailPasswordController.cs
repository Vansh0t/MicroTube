using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Constants;
using MicroTube.Controllers.Authentication.Dto;
using MicroTube.Services.Authentication;
using MicroTube.Services.Authentication.BasicFlow;
using MicroTube.Services.Cryptography;

namespace MicroTube.Controllers.Authentication
{
    [ApiController]
    [Route("Authentication/[controller]")]
    public class EmailPasswordController : ControllerBase
    {
        private readonly ILogger<EmailPasswordController> _logger;
        private readonly IBasicFlowAuthenticationProvider _basicFlowAuthentication;
		private readonly IBasicFlowEmailHandler _basicFlowEmailHandler;
		private readonly IBasicFlowPasswordHandler _basicFlowPasswordHandler;
        private readonly IJwtClaims _claims;
		private readonly IUserSessionService _userSession;
		private readonly IJwtTokenProvider _jwtAccessTokenProvider;
		private readonly IConfiguration _config;

		public EmailPasswordController(
			ILogger<EmailPasswordController> logger,
			IBasicFlowAuthenticationProvider basicFlowAuthentication,
			IJwtClaims claims,
			IUserSessionService userSession,
			IJwtTokenProvider jwtAccessTokenProvider,
			IConfiguration config,
			IBasicFlowEmailHandler basicFlowEmailHandler,
			IBasicFlowPasswordHandler basicFlowPasswordHandler)
		{
			_logger = logger;
			_basicFlowAuthentication = basicFlowAuthentication;
			_claims = claims;
			_userSession = userSession;
			_jwtAccessTokenProvider = jwtAccessTokenProvider;
			_config = config;
			_basicFlowEmailHandler = basicFlowEmailHandler;
			_basicFlowPasswordHandler = basicFlowPasswordHandler;
		}

		[HttpPost("SignUp")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthenticationResponseDto))]
        public async Task<IActionResult> SignUp(SignUpEmailPasswordDto data)
        {
            var resultCreatedUser = await _basicFlowAuthentication.CreateUser(data.Username, data.Email, data.Password);
            if (resultCreatedUser.IsError)
                return StatusCode(resultCreatedUser.Code, resultCreatedUser.Error);
            var user = resultCreatedUser.GetRequiredObject();

			var newSessionResult = await _userSession.CreateNewSession(user.Id.ToString());
			if (newSessionResult.IsError)
				return StatusCode(newSessionResult.Code, newSessionResult.Error);
			var newSession = newSessionResult.GetRequiredObject();
			HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.Expiration);
            return Ok(new AuthenticationResponseDto(newSession.AccessToken));
        }
        [HttpPost("SignIn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDto))]
        public async Task<IActionResult> SignIn(SignInCredentialPasswordDto data)
        {
            var signInResult = await _basicFlowAuthentication.SignIn(data.Credential, data.Password);
            if (signInResult.IsError)
                return StatusCode(signInResult.Code, signInResult.Error);
			var user = signInResult.GetRequiredObject();

			var newSessionResult = await _userSession.CreateNewSession(user.Id.ToString());
			if (newSessionResult.IsError)
				return StatusCode(newSessionResult.Code, newSessionResult.Error);
			var newSession = newSessionResult.GetRequiredObject();
			HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.Expiration);

			return Ok(new AuthenticationResponseDto(newSession.AccessToken));
        }
		[Authorize]
		[HttpPost("ConfirmEmailResend")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ConfirmEmail()
		{
			string userId = _claims.GetUserId(User);
			var resultJWT = await _basicFlowEmailHandler.ResendEmailConfirmation(userId);
			if (resultJWT.IsError)
				return StatusCode(resultJWT.Code, resultJWT.Error);
			return Ok();
		}
		[HttpPost("ConfirmEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDto))]
        public async Task<IActionResult> ConfirmEmail(MessageDto emailConfirmationString)
        {
            var confirmedUser = await _basicFlowEmailHandler.ConfirmEmail(emailConfirmationString.Message);
            if (confirmedUser.IsError)
                return StatusCode(confirmedUser.Code, confirmedUser.Error);

			if(User.Identity != null && User.Identity.IsAuthenticated)
			{
				var newSessionResult = await _userSession.CreateNewSession(confirmedUser.GetRequiredObject().Id.ToString());
				if (newSessionResult.IsError)
					return StatusCode(newSessionResult.Code, newSessionResult.Error);
				var newSession = newSessionResult.GetRequiredObject();
				HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.Expiration);
				return Ok(new AuthenticationResponseDto(newSession.AccessToken));
			}
			return Ok();
        }
        [Authorize]
        [HttpPost("ChangeEmailStart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeEmailStart(EmailChangeDto emailChangeData)
        {
            var userId = _claims.GetUserId(HttpContext.User);
            var resultJWT = await _basicFlowEmailHandler.StartEmailChange(userId, emailChangeData.NewEmail, emailChangeData.Password);
            if (resultJWT.IsError)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok();
        }
        [HttpPost("ResetPassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageDto))]
        public async Task<IActionResult> ResetPasswordStart(ResetPasswordStartDto resetData)
        {
            var result = await _basicFlowPasswordHandler.StartPasswordReset(resetData.Email);
            if(result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok(new MessageDto("An email will be sent to this address if an account is registered under it."));
        }
        [HttpPost("ValidatePasswordReset")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PasswordResetTokenDto))]
        public async Task<IActionResult> ValidatePasswordReset(MessageDto passwordResetString)
        {
            var result = await _basicFlowPasswordHandler.UsePasswordResetString(passwordResetString.Message);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok(new PasswordResetTokenDto(result.GetRequiredObject()));
        }
        [Authorize(AuthorizationConstants.PASSWORD_RESET_ONLY_POLICY)]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDto))]
        public async Task<IActionResult> ChangePassword(PasswordChangeDto changeData)
        {
            string userId = _claims.GetUserId(HttpContext.User);
            var result = await  _basicFlowPasswordHandler.ChangePassword(userId, changeData.NewPassword);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok();
        }
    }
}