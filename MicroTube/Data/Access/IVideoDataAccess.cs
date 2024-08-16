using MicroTube.Data.Access.SQLServer;
using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IVideoDataAccess
    {
        Task<VideoUploadProgress?> CreateUploadProgress(VideoUploadProgressCreationOptions options);
        Task<VideoUploadProgress?> GetUploadProgressById(string id);
		Task<int> UpdateUploadProgress(VideoUploadProgress uploadProgress);
		Task<VideoUploadProgress?> GetUploadProgressByFileName(string fileName);
		Task<IEnumerable<VideoUploadProgress>> GetVideoUploadProgressListForUser(string userId);
		Task<Video?> CreateVideo(Video video);
		Task<IEnumerable<Video>> GetVideos();
		Task<IEnumerable<Video>> GetVideosByIds(IEnumerable<string> ids);
		Task<Video?> GetVideo(string id);
		Task UpdateVideo(Video video);
		Task<VideoLike?> GetLike(string userId, string videoId);
		Task<VideoDislike?> GetDislike (string userId, string videoId);
		Task AddVideoView(string videoId, string ip);
		Task DeleteVideoViews(IEnumerable<string> viewIds);
		Task<IEnumerable<VideoView>> GetVideoViews();
	}
}