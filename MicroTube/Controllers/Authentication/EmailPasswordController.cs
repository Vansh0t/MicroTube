using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Authentication.Providers;

namespace MicroTube.Controllers.Authentication
{
    [ApiController]
    [Route("authentication/[controller]")]
    public class EmailPasswordController : ControllerBase
    {
        private readonly ILogger<EmailPasswordController> _logger;
        private readonly EmailPasswordAuthenticationProvider _emailPasswordAuthentication;

        public EmailPasswordController(ILogger<EmailPasswordController> logger, EmailPasswordAuthenticationProvider emailPasswordAuthentication)
        {
            _logger = logger;
            _emailPasswordAuthentication = emailPasswordAuthentication;
        }

        [HttpPost("signup")]
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
        [HttpPost("signin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponseDTO))]
        public async Task<IActionResult> SignIn(SignInCredentialPasswordDTO data)
        {
            var resultJWT = await _emailPasswordAuthentication.SignIn(data.Credential, data.Password);
            if (resultJWT.IsError || resultJWT.ResultObject == null)
                return StatusCode(resultJWT.Code, resultJWT.Error);
            return Ok(new AuthenticationResponseDTO(resultJWT.ResultObject));
        }
    }
}