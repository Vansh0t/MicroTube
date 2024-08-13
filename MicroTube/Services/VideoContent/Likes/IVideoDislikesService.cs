using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Likes
{
	public interface IVideoDislikesService
	{
		Task<IServiceResult<VideoDislike>> DislikeVideo(string userId, string videoId);
		Task<IServiceResult<VideoDislike>> GetDislike(string userId, string videoId);
		Task<IServiceResult> UndislikeVideo(string userId, string videoId);
	}
}