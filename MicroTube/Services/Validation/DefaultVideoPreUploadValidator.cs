using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Validation
{
	public class DefaultVideoPreUploadValidator : IVideoPreUploadValidator
	{
		private readonly IConfiguration _config;

		public DefaultVideoPreUploadValidator(IConfiguration config)
		{
			_config = config;
		}

		public IServiceResult Validate(IFormFile file)
		{
			var options = _config.GetRequiredSection(VideoContentUploadOptions.KEY).GetRequired<VideoContentUploadOptions>();
			if (file.Length > options.MaxFileSizeBytes)
				return ServiceResult.Fail(400, $"File is too big. Max allowed size: {options.MaxFileSizeBytes}");
			if (!options.AllowedContentTypes.Contains(file.ContentType))
				return ServiceResult.Fail(400, $"{file.ContentType} is not supported. Supported types: {string.Join(", ", options.AllowedContentTypes)}");
			string fileExtension = Path.GetExtension(file.FileName);
			if (!options.AllowedFileExtensions.Contains(fileExtension))
				return ServiceResult.Fail(400, $"File extension {fileExtension} is not supported. Supported extensions: {string.Join(", ", options.AllowedFileExtensions)}");
			return ServiceResult.Success();
		}
	}
}
