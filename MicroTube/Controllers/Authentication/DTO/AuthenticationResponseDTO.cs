namespace MicroTube.Controllers.Authentication.DTO
{
    public class AuthenticationResponseDTO
    {
        public string JWT { get; set; }
		//public string RefreshToken { get; set; }
		public AuthenticationResponseDTO(string jwt/*, string refreshToken*/)
		{
			JWT = jwt;
			//RefreshToken = refreshToken;
		}
	}
}
