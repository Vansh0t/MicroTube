using Ardalis.GuardClauses;
using Azure.Storage.Blobs.Models;
using MicroTube.Services.ContentStorage;
using System.IO.Abstractions;

namespace MicroTube.Services.VideoContent.Preprocessing.Stages
{
	public class EnsureVideoSourceInRemoteStorage : VideoPreprocessingStage
	{
		private readonly IVideoFileNameGenerator _videoNameGenerator;
		private readonly IFileSystem _fileSystem;
		private readonly IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> _videoRemoteStorage;
		private readonly IRemoteLocationNameGenerator _remoteLocationNameGenerator;
		public EnsureVideoSourceInRemoteStorage(
			IVideoFileNameGenerator videoNameGenerator,
			IFileSystem fileSystem,
			IRemoteLocationNameGenerator remoteLocationNameGenerator,
			IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions> videoRemoteStorage)
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
			var accessOptions = new AzureBlobAccessOptions(context.PreprocessingData.GeneratedSourceFileName, context.PreprocessingData.GeneratedSourceFileLocation);
			bool videoSourceExists = await _videoRemoteStorage.FileExists(accessOptions, cancellationToken);
			if(!videoSourceExists)
			{
				throw new RequiredObjectNotFoundException("Video was not uploaded.");
			}
			context.RemoteCache = new Processing.Stages.VideoProcessingRemoteCache 
			{ 
				VideoFileLocation = accessOptions.ContainerName,
				VideoFileName = accessOptions.FileName
			};
			return context;
		}
	}
}
