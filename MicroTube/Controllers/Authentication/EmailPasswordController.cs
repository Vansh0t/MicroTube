using Microsoft.AspNetCore.Mvc;
using MicroTube.Controllers.Authentication.DTO;
using MicroTube.Services.Authentication.Providers;
using System.Reflection;

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
        public async Task<IActionResult> SignUp(SignUpEmailPasswordDTO data)
        {
            var resultUser = await _emailPasswordAuthentication.CreateUser(data.Username, data.Email, data.Password);
            if (resultUser.IsError)
                return StatusCode(resultUser.Code, resultUser.Error);
            return Redirect("/");
        }
    }
}