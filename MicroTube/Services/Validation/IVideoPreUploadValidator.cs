namespace MicroTube.Services.Validation
{
	public interface IVideoPreUploadValidator
	{
		IServiceResult ValidateFile(IFormFile file);
		IServiceResult ValidateTitle(string? title);
		IServiceResult ValidateDescription(string? description);
	}
}