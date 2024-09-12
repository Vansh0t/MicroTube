﻿using MicroTube.Services.MediaContentStorage;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Processing.Stages.Offline
{
	public class OfflineUploadVideoToCdnStage : VideoProcessingStage
	{
		private readonly ICdnMediaContentAccess _mediaCdnAccess;
		private readonly IFileSystem _fileSystem;

		public OfflineUploadVideoToCdnStage(ICdnMediaContentAccess mediaCdnAccess)
		{
			_mediaCdnAccess = mediaCdnAccess;
		}

		protected override async Task<DefaultVideoProcessingContext> ExecuteInternal(DefaultVideoProcessingContext? context, CancellationToken cancellationToken)
		{
			if (context == null)
			{
				throw new ArgumentNullException($"Context must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.LocalCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.LocalCache)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			if (context.RemoteCache == null)
			{
				throw new ArgumentNullException($"{nameof(context.RemoteCache)} must not be null for stage {nameof(OfflineUploadVideoToCdnStage)}");
			}
			IEnumerable<string> qualityTierPaths = GetQualityTierPaths(context.LocalCache.QualityTiersLocation);
			List<Task<Uri>> uploadTasks = new List<Task<Uri>>();
			foreach(var tierPath in qualityTierPaths)
			{
				var task = UploadVideoToCdn(tierPath, _fileSystem.Path.GetFileName(tierPath), cancellationToken);
				uploadTasks.Add(task);
			}
			await Task.WhenAll(uploadTasks);
			IEnumerable<Uri> videoUrls = uploadTasks.Where(_ => _.IsCompletedSuccessfully).Select(_ => _.Result);
			if (context.Cdn == null)
				context.Cdn = new Cdn();
			context.Cdn.VideoEndpoints = videoUrls;
			return context;
		}
		private async Task<Uri> UploadVideoToCdn(string localCacheSourceVideoPath, string videoName, CancellationToken cancellationToken)
		{
			using var fileStream = _fileSystem.FileStream.New(localCacheSourceVideoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			var videoUploadResult = await _mediaCdnAccess.UploadVideo(fileStream, videoName, cancellationToken);
			if (videoUploadResult.IsError)
			{
				throw new BackgroundJobException($"Failed to upload video file to CDN. Path: {localCacheSourceVideoPath}. {videoUploadResult.Error}");
			}
			return videoUploadResult.GetRequiredObject();
		}
		private IEnumerable<string> GetQualityTierPaths(string containingLocation)
		{
			return _fileSystem.Directory.GetFiles(containingLocation);
		}
	}
}