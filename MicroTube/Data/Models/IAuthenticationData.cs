namespace MicroTube.Data.Models
{
	public interface IAuthenticationData
	{
		public Guid UserId { get; set; }
		public AppUser? User { get; set; }
	}
}
