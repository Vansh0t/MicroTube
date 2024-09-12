using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.IO.Abstractions;

namespace MicroTube.Services.MediaContentStorage
{
	public class AzureBlobVideoContentRemoteStorage : IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>
	{
		private readonly IConfiguration _config;
		private readonly BlobServiceClient _azureBlobServiceClient;
		private readonly ILogger<AzureBlobVideoContentRemoteStorage> _logger;
		private readonly IFileSystem _fileSystem;

		public AzureBlobVideoContentRemoteStorage(
			IConfiguration config,
			BlobServiceClient azureBlobServiceClient,
			ILogger<AzureBlobVideoContentRemoteStorage> logger,
			IFileSystem fileSystem)
		{
			_config = config;
			_azureBlobServiceClient = azureBlobServiceClient;
			_logger = logger;
			_fileSystem = fileSystem;
		}
		public async Task<IServiceResult<string>> Upload(Stream stream, AzureBlobAccessOptions accessOptions, BlobUploadOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			await blobContainerClient.CreateIfNotExistsAsync();
			var blobClient = blobContainerClient.GetBlockBlobClient(accessOptions.FileName);
			uploadOptions = EnsureCorrectContentType(accessOptions.FileName, uploadOptions);
			var response = await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
			var httpResponse = response.GetRawResponse();
			if (httpResponse.IsError)
			{
				_logger.LogError("Failed to upload media content to Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult<string>.FailInternal();
			}
			return ServiceResult<string>.Success(accessOptions.FileName);
		}
		public async Task<IServiceResult<IEnumerable<string>>> Upload(string path, AzureBlobAccessOptions accessOptions, BlobUploadOptions uploadOptions, CancellationToken cancellationToken)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			FileAttributes fileAttributes = _fileSystem.File.GetAttributes(path);
			if(fileAttributes == FileAttributes.Directory)
			{
				var uploadedFileNames = await UploadAllFilesFromDirectory(path, blobContainerClient, uploadOptions, cancellationToken);
				return ServiceResult<IEnumerable<string>>.Success(uploadedFileNames);
			}
			var blobClient = blobContainerClient.GetBlobClient(accessOptions.FileName);
			uploadOptions = EnsureCorrectContentType(accessOptions.FileName, uploadOptions);
			var response = await blobClient.UploadAsync(path, uploadOptions, cancellationToken);
			var httpResponse = response.GetRawResponse();
			if (httpResponse.IsError)
			{
				_logger.LogError("Failed to upload media content to Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult<IEnumerable<string>>.FailInternal();
			}
			return ServiceResult<IEnumerable<string>>.Success(new string[1] { accessOptions.FileName });
		}
		public async Task<IServiceResult> Delete(AzureBlobAccessOptions options, CancellationToken cancellationToken = default)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(options.ContainerName);
			var blobClient = blobContainerClient.GetBlockBlobClient(options.FileName);
			var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
			var httpResponse = response.GetRawResponse(); //httpResponse is null here if container already exists for some reason
			if (httpResponse != null && httpResponse.IsError)
			{
				_logger.LogError("Failed to delete media content from Azure blob storage. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
		public async Task<IServiceResult<string>> Download(string saveToPath, AzureBlobAccessOptions options, CancellationToken cancellationToken = default)
		{
			_fileSystem.Directory.CreateDirectory(saveToPath);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(options.ContainerName);
			var blobClient = blobContainerClient.GetBlobClient(options.FileName);
			var response = await blobClient.DownloadToAsync(Path.Join(saveToPath, options.FileName), cancellationToken);
			if (response.IsError)
			{
				_logger.LogError("Failed to download media content from Azure blob storage. {statusCode}, {reasonPhrase}", response.Status, response.ReasonPhrase);
				return ServiceResult<string>.FailInternal();
			}
			return ServiceResult<string>.Success(Path.Join(saveToPath, options.FileName));
		}
		public async Task<IServiceResult> EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(locationName);
			PublicAccessType blobContainerAccess = PublicAccessType.None;
			if (locationAccess == RemoteLocationAccess.Public)
				blobContainerAccess = PublicAccessType.Blob;
			var response = await blobContainerClient.CreateIfNotExistsAsync(blobContainerAccess, cancellationToken: cancellationToken);
			var httpResponse = response.GetRawResponse(); //httpResponse is null here if container already exists for some reason
			if (httpResponse != null && httpResponse.IsError)
			{
				_logger.LogError("Failed to ensure location. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
		private async Task<IEnumerable<string>> UploadAllFilesFromDirectory(string directory, BlobContainerClient containerClient, BlobUploadOptions uploadOptions, CancellationToken cancellationToken)
		{
			string[] directoryFiles = _fileSystem.Directory.GetFiles(directory);
			if (directoryFiles.Length == 0)
			{
				_logger.LogWarning("Tried to upload directory that does not contain files {directory}", directory);
				return directoryFiles.Select(_ => _fileSystem.Path.GetFileName(_));
			}
			List<UploadTaskContainer> uploadingTasks = new List<UploadTaskContainer>();
			foreach(var file in directoryFiles)
			{
				string fileName = _fileSystem.Path.GetFileName(file);
				var blobClient = containerClient.GetBlobClient(fileName);
				uploadOptions = EnsureCorrectContentType(fileName, uploadOptions, true);
				Task<Response<BlobContentInfo>> task = blobClient.UploadAsync(file, uploadOptions, cancellationToken);
				var taskContainer = new UploadTaskContainer(task, fileName);
				uploadingTasks.Add(taskContainer);
			}
			await Task.WhenAll(uploadingTasks.Select(_=>_.Task));
			var errorUploads = uploadingTasks.Where(_ => _.Task.Result.GetRawResponse().IsError).Select(_=>_.Task.Result.GetRawResponse());
			foreach(var errorUpload in errorUploads)
			{
				_logger.LogError("File upload error, {statusCode}, {reasonPhrase}.", errorUpload.Status, errorUpload.ReasonPhrase);
			}
			return uploadingTasks.Where(_ => !_.Task.Result.GetRawResponse().IsError).Select(_ => _.FileName);
		}

		public async Task<IServiceResult> DeleteLocation(string locationName, CancellationToken cancellationToken)
		{
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(locationName);
			var response = await blobContainerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
			var httpResponse = response.GetRawResponse(); //httpResponse is null here if container already exists for some reason
			if (httpResponse != null && httpResponse.IsError)
			{
				_logger.LogError("Failed to delete location. {statusCode}, {reasonPhrase}", httpResponse.Status, httpResponse.ReasonPhrase);
				return ServiceResult.FailInternal();
			}
			return ServiceResult.Success();
		}
		private BlobUploadOptions EnsureCorrectContentType(string fileName, BlobUploadOptions blobUploadOptions, bool alwaysOverride = false)
		{
			var headers = blobUploadOptions.HttpHeaders;
			if(headers == null)
			{
				headers = new BlobHttpHeaders();
				blobUploadOptions.HttpHeaders = headers;
			}
			if(alwaysOverride || string.IsNullOrWhiteSpace(headers.ContentType))
			{
				headers.ContentType = MimeMapping.MimeUtility.GetMimeMapping(fileName);
			}
			return blobUploadOptions;
		}
		private readonly struct UploadTaskContainer
		{
			public Task<Azure.Response<BlobContentInfo>> Task { get; }
			public string FileName { get; }
			public UploadTaskContainer(Task<Response<BlobContentInfo>> task, string fileName)
			{
				Task = task;
				FileName = fileName;
			}
		}
	}
}
