namespace MicroTube.Services.Validation
{
	public interface IVideoPreUploadValidator
	{
		IServiceResult Validate(IFormFile file);
	}
}