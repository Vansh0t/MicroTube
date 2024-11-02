namespace MicroTube.Services.Validation
{
	public interface ICommentContentValidator
	{
		IServiceResult Validate(string? content);
	}
}