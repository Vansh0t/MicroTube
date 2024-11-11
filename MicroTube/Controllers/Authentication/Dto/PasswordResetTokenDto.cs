namespace MicroTube.Controllers.Authentication.Dto
{
    public class PasswordResetTokenDto
	{
        public string JWT { get; set; }
		public PasswordResetTokenDto(string jwt)
		{
			JWT = jwt;
		}
	}
}
