using System.IO.Abstractions;

namespace MicroTube.Services.MediaContentStorage
{
	public class OfflineVideoContentRemoteStorage : IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions>
	{
		private readonly IFileSystem _fileSystem;

		public OfflineVideoContentRemoteStorage(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public Task<IServiceResult> Delete(OfflineRemoteStorageOptions options, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(DeleteFromLocal(options.FullPath));
		}

		public Task<IServiceResult> DeleteLocation(string locationName, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(DeleteFromLocal(locationName));
		}

		public Task<IServiceResult<string>> Download(string saveToPath, OfflineRemoteStorageOptions options, CancellationToken cancellationToken = default)
		{
			_fileSystem.Directory.CreateDirectory(saveToPath);
			string saveToFileFullPath = _fileSystem.Path.Join(saveToPath, _fileSystem.Path.GetFileName(options.FullPath));
			var result = CopyLocally(options.FullPath, saveToFileFullPath);
			if(result.IsError)
			{
				return Task.FromResult<IServiceResult<string>>(ServiceResult<string>.Fail(result.Code, result.Error!));
			}
			return Task.FromResult<IServiceResult<string>>(ServiceResult<string>.Success(saveToFileFullPath));
		}

		public Task<IServiceResult> EnsureLocation(string locationName, RemoteLocationAccess locationAccess, CancellationToken cancellationToken = default)
		{
			_fileSystem.Directory.CreateDirectory(locationName);
			return Task.FromResult<IServiceResult>(ServiceResult.Success());
		}

		public async Task<IServiceResult<string>> Upload(Stream stream, OfflineRemoteStorageOptions accessOptions, OfflineRemoteStorageOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			try
			{
				using var fileStream = _fileSystem.FileStream.New(uploadOptions.FullPath, FileMode.Create);
				await stream.CopyToAsync(fileStream);
				return ServiceResult<string>.Success(_fileSystem.Path.GetFileName(accessOptions.FullPath));
			}
			catch (Exception e)
			{
				return ServiceResult<string>.Fail(500, e.Message + " " + e.StackTrace);
			}
			
		}

		public Task<IServiceResult<IEnumerable<string>>> Upload(string path, OfflineRemoteStorageOptions accessOptions, OfflineRemoteStorageOptions uploadOptions, CancellationToken cancellationToken = default)
		{
			FileAttributes fileAttributes = _fileSystem.File.GetAttributes(path);
			List<string> resultPaths = new();
			if (fileAttributes == FileAttributes.Directory)
			{
				foreach(var filePath in _fileSystem.Directory.GetFiles(path))
				{
					string fileName = _fileSystem.Path.GetFileName(filePath);
					string uploadFileFullPath = _fileSystem.Path.Join(uploadOptions.FullPath, fileName);
					var fileResult = CopyLocally(filePath, uploadFileFullPath);
					if(!fileResult.IsError)
					{
						resultPaths.Add(uploadFileFullPath);
					}
				}
			}
			else
			{
				string fileName = _fileSystem.Path.GetFileName(path);
				string uploadFileFullPath = _fileSystem.Path.Join(uploadOptions.FullPath, fileName);
				var result = CopyLocally(path, uploadOptions.FullPath);
				if (result.IsError)
				{
					return Task.FromResult<IServiceResult<IEnumerable<string>>>(ServiceResult<IEnumerable<string>>.Fail(result.Code, result.Error!));
				}
				resultPaths.Add(uploadFileFullPath);
			}
			
			
			return Task.FromResult<IServiceResult<IEnumerable<string>>>(ServiceResult<IEnumerable<string>>.Success(resultPaths));
		}

		private IServiceResult DeleteFromLocal(string path)
		{
			try
			{
				if (_fileSystem.File.Exists(path))
					_fileSystem.File.Delete(path);
				return ServiceResult.Success();
			}
			catch (Exception e)
			{
				return ServiceResult.Fail(500, e.Message + " " + e.StackTrace);
			}
		}
		private IServiceResult CopyLocally(string fromLocation, string toLocation)
		{
			try
			{
				_fileSystem.File.Copy(fromLocation, toLocation, true);
				return ServiceResult.Success();
			}
			catch (Exception e)
			{
				return ServiceResult.Fail(500, e.Message + " " + e.StackTrace);
			}
		}
	}
}
