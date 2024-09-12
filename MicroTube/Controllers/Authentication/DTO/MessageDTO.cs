namespace MicroTube.Controllers.Authentication.DTO
{
	public class MessageDTO
	{
		public string Message { get; set; }
		public MessageDTO(string message)
		{
			Message = message;
		}
	}
}
