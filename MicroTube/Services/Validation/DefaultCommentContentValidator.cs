namespace MicroTube.Services.Validation
{
	public class DefaultCommentContentValidator : ICommentContentValidator
	{
		private const int MAX_CONTENT_LENGTH = 512;
		public IServiceResult Validate(string? content)
		{
			if (string.IsNullOrWhiteSpace(content) || content.Length > MAX_CONTENT_LENGTH)
			{
				return ServiceResult.Fail(400, $"Comment content must not be empty and not longer than {MAX_CONTENT_LENGTH} characters");
			}
			return ServiceResult.Success();
		}
	}
}
