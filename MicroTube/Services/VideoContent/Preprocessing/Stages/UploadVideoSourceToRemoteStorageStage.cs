using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.MediaContentStorage;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
	public class UploadVideoSourceToRemoteStorageStage : VideoPreprocessingStage
	{
		private readonly IVideoNameGenerator _videoNameGenerator;
		private readonly IFileSystem _fileSystem;
		private readonly IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IRemoteLocationNameGenerator _remoteLocationNameGenerator;
		public UploadVideoSourceToRemoteStorageStage(
			IVideoNameGenerator videoNameGenerator,
			IFileSystem fileSystem,
			IRemoteLocationNameGenerator remoteLocationNameGenerator,
			IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage)
		{
			_videoNameGenerator = videoNameGenerator;
			_fileSystem = fileSystem;
			_remoteLocationNameGenerator = remoteLocationNameGenerator;
			_videoRemoteStorage = videoRemoteStorage;
		}

		protected override async Task<DefaultVideoPreprocessingContext> ExecuteInternal(DefaultVideoPreprocessingContext? context, CancellationToken cancellationToken)
		{
			Guard.Against.Null(context);
			Guard.Against.Null(context.PreprocessingData);
			string generatedFileName = _videoNameGenerator.GenerateVideoName() + _fileSystem.Path.GetExtension(context.PreprocessingData.VideoFile.FileName);
			string generatedRemoteLocationName = _remoteLocationNameGenerator.GetLocationName(generatedFileName);
			using var stream = context.PreprocessingData.VideoFile.OpenReadStream();
			var blobUploadOptions = new BlobUploadOptions
			{
				AccessTier = AccessTier.Cold
			};
			await _videoRemoteStorage.EnsureLocation(generatedFileName, RemoteLocationAccess.Public, cancellationToken);
			string uploadedFileName = await _videoRemoteStorage.Upload(stream, 
				new AzureBlobAccessOptions(generatedFileName, generatedRemoteLocationName), new BlobUploadOptions { AccessTier = AccessTier.Cold });
			context.RemoteCache = new Processing.Stages.VideoProcessingRemoteCache { VideoFileLocation = generatedRemoteLocationName, VideoFileName = uploadedFileName };
			return context;
		}
	}
}
