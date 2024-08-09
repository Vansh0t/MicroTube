namespace MicroTube.Services.Search
{
	public interface ISearchMetaProvider<TInput, TMeta>
	{
		string SerializeMeta(TMeta data);
		TMeta? BuildMeta(TInput data);
		TMeta? DeserializeMeta(string? data);
	}
}