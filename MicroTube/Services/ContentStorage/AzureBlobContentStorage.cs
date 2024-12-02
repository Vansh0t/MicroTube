using Ardalis.GuardClauses;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace MicroTube.Services.ContentStorage
{
	public class AzureBlobContentStorage : IRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>
	{
		private readonly BlobServiceClient _azureBlobServiceClient;
		private readonly ILogger<AzureBlobContentStorage> _logger;
		private readonly IFileSystem _fileSystem;

		public AzureBlobContentStorage(
			BlobServiceClient azureBlobServiceClient,
			ILogger<AzureBlobContentStorage> logger,
			IFileSystem fileSystem)
		{
			_azureBlobServiceClient = azureBlobServiceClient;
			_logger = logger;
			_fileSystem = fileSystem;
		}
		public async Task<string> Upload(Stream stream, AzureBlobAccessOptions accessOptions, BlobUploadOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(accessOptions.ContainerName);
			Guard.Against.NullOrWhiteSpace(accessOptions.FileName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			await blobContainerClient.CreateIfNotExistsAsync();
			var blobClient = blobContainerClient.GetBlockBlobClient(accessOptions.FileName);
			uploadOptions = EnsureCorrectContentType(accessOptions.FileName, uploadOptions);
			var response = await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
			var httpResponse = response.GetRawResponse();
			ThrowIfErrorResponse(response.GetRawResponse());
			return accessOptions.FileName;
		}
		public async Task<string> Upload(string filePath, AzureBlobAccessOptions accessOptions, BlobUploadOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(filePath);
			Guard.Against.NullOrWhiteSpace(accessOptions.ContainerName);
			Guard.Against.NullOrWhiteSpace(accessOptions.FileName);
			FileAttributes fileAttributes = _fileSystem.File.GetAttributes(filePath);
			if(fileAttributes == FileAttributes.Directory)
			{
				throw new ArgumentException($"Target file is a directory. Call {nameof(UploadDirectory)} if you want to upload a directory.");
			}
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			var blobClient = blobContainerClient.GetBlobClient(accessOptions.FileName);
			uploadOptions = EnsureCorrectContentType(accessOptions.FileName, uploadOptions);
			var response = await blobClient.UploadAsync(filePath, uploadOptions, cancellationToken);
			ThrowIfErrorResponse(response.GetRawResponse());
			return accessOptions.FileName;
		}
		public async Task<BulkUploadResult> UploadDirectory(string directoryPath, AzureBlobAccessOptions accessOptions, BlobUploadOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(directoryPath);
			Guard.Against.NullOrWhiteSpace(accessOptions.ContainerName);

			FileAttributes fileAttributes = _fileSystem.File.GetAttributes(directoryPath);
			if (fileAttributes != FileAttributes.Directory)
			{
				throw new ArgumentException($"Target file is not a directory. Call {nameof(Upload)} if you want to upload a file.");
			}
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			string[] directoryFiles = _fileSystem.Directory.GetFiles(directoryPath);
			List<UploadTaskContainer> uploadingTasks = new List<UploadTaskContainer>();
			var successes = new List<string>();
			var fails = new List<string>();

			foreach (var file in directoryFiles)
			{
				string fileName = _fileSystem.Path.GetFileName(file);
				try
				{
					var blobClient = blobContainerClient.GetBlobClient(fileName);
					uploadOptions = EnsureCorrectContentType(fileName, uploadOptions, true);
					var stream = _fileSystem.FileStream.New(file, FileMode.Open);
					Task<Response<BlobContentInfo>> task = blobClient.UploadAsync(stream, uploadOptions, cancellationToken);
					var taskContainer = new UploadTaskContainer(task, fileName, stream);
					uploadingTasks.Add(taskContainer);
				}
				catch(Exception e)
				{
					_logger.LogError($"File upload error. {e}");
					fails.Add(fileName);
				}
			}
			await Task.WhenAll(uploadingTasks.Select(_ => _.Task));
			var errorUploads = uploadingTasks.Where(_ => _.Task.Result.GetRawResponse().IsError).Select(_ => _.Task.Result.GetRawResponse());
			foreach (var errorUpload in errorUploads)
			{
				_logger.LogError("File upload error, {statusCode}, {reasonPhrase}.", errorUpload.Status, errorUpload.ReasonPhrase);
			}
			successes = successes.Concat(uploadingTasks.Where(_ => !_.Task.Result.GetRawResponse().IsError).Select(_=>_.FileName)).ToList();
			fails = fails.Concat(uploadingTasks.Where(_ => _.Task.Result.GetRawResponse().IsError).Select(_=>_.FileName)).ToList();
			foreach (var taskContainer in uploadingTasks)
				taskContainer.Stream.Dispose();
			return new BulkUploadResult(successes, fails);
		}
		public async Task Delete(AzureBlobAccessOptions options, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(options.ContainerName);
			Guard.Against.NullOrWhiteSpace(options.FileName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(options.ContainerName);
			var blobClient = blobContainerClient.GetBlockBlobClient(options.FileName);
			var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
			ThrowIfErrorResponse(response.GetRawResponse());
		}
		public async Task<string> Download(string saveToPath, AzureBlobAccessOptions options, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(saveToPath);
			Guard.Against.NullOrWhiteSpace(options.ContainerName);
			Guard.Against.NullOrWhiteSpace(options.FileName);
			_fileSystem.Directory.CreateDirectory(saveToPath);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(options.ContainerName);
			var blobClient = blobContainerClient.GetBlobClient(options.FileName);
			using Stream downloadStream = _fileSystem.FileStream.New(Path.Join(saveToPath, options.FileName), FileMode.Create);
			var response = await blobClient.DownloadToAsync(downloadStream, cancellationToken);
			ThrowIfErrorResponse(response);
			return Path.Join(saveToPath, options.FileName);
		}
		public async Task EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(locationName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(locationName);
			PublicAccessType blobContainerAccess = PublicAccessType.None;
			if (locationAccess == RemoteLocationAccess.Public)
				blobContainerAccess = PublicAccessType.Blob;
			var response = await blobContainerClient.CreateIfNotExistsAsync(blobContainerAccess, cancellationToken: cancellationToken);
			ThrowIfErrorResponse(response.GetRawResponse());
		}
		public async Task DeleteLocation(string locationName, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(locationName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(locationName);
			var response = await blobContainerClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
			ThrowIfErrorResponse(response.GetRawResponse());
		}
		public async Task<bool> FileExists(AzureBlobAccessOptions accessOptions, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(accessOptions.FileName);
			Guard.Against.NullOrWhiteSpace(accessOptions.ContainerName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			var blobClient = blobContainerClient.GetBlobClient(accessOptions.FileName);
			var existsResponse = await blobClient.ExistsAsync();
			return existsResponse.Value;
		}
		public async Task<IDictionary<string, string?>> GetFileMetadata(AzureBlobAccessOptions accessOptions, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(accessOptions.FileName);
			Guard.Against.NullOrWhiteSpace(accessOptions.ContainerName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(accessOptions.ContainerName);
			var blobClient = blobContainerClient.GetBlobClient(accessOptions.FileName);
			var propertiesResponse = await blobClient.GetPropertiesAsync();
			BlobProperties properties = propertiesResponse.Value;
			return properties.Metadata;
		}
		public async Task<IDictionary<string, string?>> GetLocationMetadata(string locationName, CancellationToken cancellationToken = default)
		{
			Guard.Against.NullOrWhiteSpace(locationName);
			var blobContainerClient = _azureBlobServiceClient.GetBlobContainerClient(locationName);
			var propertiesResponse = await blobContainerClient.GetPropertiesAsync();
			BlobContainerProperties properties = propertiesResponse.Value;
			return properties.Metadata;
		}
		private void ThrowIfErrorResponse(Response response)
		{
			if (response != null && response.IsError) //httpResponse is null here if container already exists for some reason
			{
				throw new ExternalServiceException($"Failed to delete location. {response.Status}, {response.ReasonPhrase}");
			}
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
			public Stream Stream { get; }
			public UploadTaskContainer(Task<Response<BlobContentInfo>> task, string fileName, Stream stream)
			{
				Task = task;
				FileName = fileName;
				Stream = stream;
			}
		}
	}
}
