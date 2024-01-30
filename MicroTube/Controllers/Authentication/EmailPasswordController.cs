﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroTube.Constants;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Authentication;
using MicroTube.Services.Authentication.Providers;
using MicroTube.Services.Cryptography;

namespace MicroTube.Controllers.Authentication
{
    [ApiController]
    [Route("Authentication/[controller]")]
    public class EmailPasswordController : ControllerBase
    {
        private readonly ILogger<EmailPasswordController> _logger;
        private readonly EmailPasswordAuthenticationProvider _emailPasswordAuthentication;
        private readonly IJwtClaims _claims;
		private readonly IUserSessionService _userSession;
		private readonly IJwtTokenProvider _jwtAccessTokenProvider;
		private readonly IConfiguration _config;

		public EmailPasswordController(
			ILogger<EmailPasswordController> logger,
			EmailPasswordAuthenticationProvider emailPasswordAuthentication,
			IJwtClaims claims,
			IUserSessionService userSession,
			IJwtTokenProvider jwtAccessTokenProvider,
			IConfiguration config)
		{
			_logger = logger;
			_emailPasswordAuthentication = emailPasswordAuthentication;
			_claims = claims;
			_userSession = userSession;
			_jwtAccessTokenProvider = jwtAccessTokenProvider;
			_config = config;
		}

		[HttpPost("SignUp")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> SignUp(SignUpEmailPasswordDTO data)
        {
            var resultCreatedUser = await _emailPasswordAuthentication.CreateUser(data.Username, data.Email, data.Password);
            if (resultCreatedUser.IsError)
                return StatusCode(resultCreatedUser.Code, resultCreatedUser.Error);
            var user = resultCreatedUser.GetRequiredObject();

			var newSessionResult = await _userSession.CreateNewSession(user.Id.ToString());
			if (newSessionResult.IsError)
				return StatusCode(newSessionResult.Code, newSessionResult.Error);
			var newSession = newSessionResult.GetRequiredObject();
			HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.ExpirationDateTime);
            return Ok(new AuthenticationResponseDTO(newSession.AccessToken));
        }
        [HttpPost("SignIn")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> SignIn(SignInCredentialPasswordDTO data)
        {
            var signInResult = await _emailPasswordAuthentication.SignIn(data.Credential, data.Password);
            if (signInResult.IsError)
                return StatusCode(signInResult.Code, signInResult.Error);
			var user = signInResult.GetRequiredObject();

			var newSessionResult = await _userSession.CreateNewSession(user.Id.ToString());
			if (newSessionResult.IsError)
				return StatusCode(newSessionResult.Code, newSessionResult.Error);
			var newSession = newSessionResult.GetRequiredObject();
			HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.ExpirationDateTime);

			return Ok(new AuthenticationResponseDTO(newSession.AccessToken));
        }
		[Authorize]
		[HttpPost("ConfirmEmailResend")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ConfirmEmail()
		{
			string userId = _claims.GetUserId(User);
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
            var confirmedUser = await _emailPasswordAuthentication.ConfirmEmail(emailConfirmationString.Message);
            if (confirmedUser.IsError)
                return StatusCode(confirmedUser.Code, confirmedUser.Error);

			if(User.Identity != null && User.Identity.IsAuthenticated)
			{
				var newSessionResult = await _userSession.CreateNewSession(confirmedUser.GetRequiredObject().Id.ToString());
				if (newSessionResult.IsError)
					return StatusCode(newSessionResult.Code, newSessionResult.Error);
				var newSession = newSessionResult.GetRequiredObject();
				HttpContext.AddRefreshTokenCookie(_config, newSession.RefreshTokenRaw, newSession.Session.ExpirationDateTime);
				return Ok(new AuthenticationResponseDTO(newSession.AccessToken));
			}
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PasswordResetTokenDTO))]
        public async Task<IActionResult> ValidatePasswordReset(MessageDTO passwordResetString)
        {
            var result = await _emailPasswordAuthentication.UsePasswordResetString(passwordResetString.Message);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok(new PasswordResetTokenDTO(result.GetRequiredObject()));
        }
        [Authorize(AuthorizationConstants.PASSWORD_RESET_ONLY_POLICY)]
        [HttpPost("ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> ChangePassword(PasswordChangeDTO changeData)
        {
            string userId = _claims.GetUserId(HttpContext.User);
            var result = await  _emailPasswordAuthentication.ChangePassword(userId, changeData.NewPassword);
            if (result.IsError)
                return StatusCode(result.Code, result.Error);
            return Ok();
        }
    }
}