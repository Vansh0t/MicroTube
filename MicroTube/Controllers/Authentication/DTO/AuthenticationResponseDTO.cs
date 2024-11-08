namespace MicroTube.Controllers.Authentication.Dto
{
    public class AuthenticationResponseDto
    {
        public string JWT { get; set; }
		//public string RefreshToken { get; set; }
		public AuthenticationResponseDto(string jwt/*, string refreshToken*/)
		{
			JWT = jwt;
			//RefreshToken = refreshToken;
		}
	}
}
