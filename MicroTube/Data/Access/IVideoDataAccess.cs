using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IVideoDataAccess
    {
        Task<VideoUploadProgress?> CreateUploadProgress(string localFullPath, string uploaderId, string title, string? description);
        Task<VideoUploadProgress?> GetUploadProgressById(string id);
		Task<int> UpdateUploadStatus(string id, VideoUploadStatus status);
		Task<VideoUploadProgress?> GetUploadProgressByLocalFullPath(string localFullPath);
		Task<IEnumerable<VideoUploadProgress>> GetVideoUploadProgressListForUser(string userId);

	}
}