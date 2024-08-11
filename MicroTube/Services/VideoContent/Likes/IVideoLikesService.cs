using MicroTube.Data.Models;

namespace MicroTube.Services.VideoContent.Likes
{
	public interface IVideoLikesService
	{
		Task<IServiceResult<VideoLike>> LikeVideo(string userId, string videoId);
		Task<IServiceResult<VideoLike>> GetLike(string userId, string videoId);
	}
}