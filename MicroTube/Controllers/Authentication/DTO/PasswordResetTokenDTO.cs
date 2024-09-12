namespace MicroTube.Controllers.Authentication.DTO
{
    public class PasswordResetTokenDTO
	{
        public string JWT { get; set; }
		public PasswordResetTokenDTO(string jwt)
		{
			JWT = jwt;
		}
	}
}
