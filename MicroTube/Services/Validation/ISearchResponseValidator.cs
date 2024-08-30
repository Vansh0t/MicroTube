namespace MicroTube.Services.Validation
{
	public interface ISearchResponseValidator<TResponse>
	{
		bool Validate(TResponse response);
	}
}