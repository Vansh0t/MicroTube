using MicroTube.Services.ConfigOptions;

namespace MicroTube.Services.Validation
{
	public class DefaultVideoPreUploadValidator : IVideoPreUploadValidator
	{
		private const int MAX_TITLE_LENGTH = 50;
		private const int MIN_TITLE_LENGTH = 2;
		private const int MAX_DESCRIPTION_LENGTH = 200;
		private readonly IConfiguration _config;

		public DefaultVideoPreUploadValidator(IConfiguration config)
		{
			_config = config;
		}

		public IServiceResult ValidateDescription(string? description)
		{
			if (description != null && description.Length > MAX_DESCRIPTION_LENGTH)
				return ServiceResult.Fail(400, "Max description length: " + MAX_DESCRIPTION_LENGTH);
			return ServiceResult.Success();
		}

		public IServiceResult ValidateFile(IFormFile file)
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

		public IServiceResult ValidateTitle(string? title)
		{
			var failResult = ServiceResult.Fail(400, $"Invalid title. Title must be {MIN_TITLE_LENGTH}-{MAX_TITLE_LENGTH} characters long");
			if(string.IsNullOrWhiteSpace(title))
			{
				return failResult;
			}
			if(title.Length < MIN_TITLE_LENGTH || title.Length > MAX_TITLE_LENGTH)
			{
				return failResult;
			}
			return ServiceResult.Success();
		}
	}
}
