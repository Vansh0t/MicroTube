using MicroTube.Data.Access.SQLServer;
using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IVideoDataAccess
    {
        Task<VideoUploadProgress?> CreateUploadProgress(VideoUploadProgressCreationOptions options);
        Task<VideoUploadProgress?> GetUploadProgressById(string id);
		Task<int> UpdateUploadProgress(string id, VideoUploadStatus status, string? message = null);
		Task<VideoUploadProgress?> GetUploadProgressByLocalFullPath(string localFullPath);
		Task<IEnumerable<VideoUploadProgress>> GetVideoUploadProgressListForUser(string userId);

	}
}