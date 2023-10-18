namespace MicroTube.Controllers.Authentication.DTO
{
    public class AuthenticationResponseDTO
    {
        public string JWT { get; set; }
        public AuthenticationResponseDTO(string jwt)
        {
            JWT = jwt;
        }
    }
}
