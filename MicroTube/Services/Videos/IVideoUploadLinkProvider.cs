
using MicroTube.Controllers.Videos.Dto;

namespace MicroTube.Services.Videos
{
	public interface IVideoUploadLinkProvider
	{
		Task<IServiceResult<VideoUploadLinkDto>> GetUploadLink(string fileName, Dictionary<string, string?> meta);
	}
}