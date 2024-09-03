namespace MicroTube.Services.MediaContentStorage
{
	public class BulkUploadResult
	{
		public BulkUploadResult(IEnumerable<string> successfulUploadsFileNames, IEnumerable<string> failedUploadsFileNames)
		{
			SuccessfulUploadsFileNames = successfulUploadsFileNames;
			FailedUploadsFileNames = failedUploadsFileNames;
		}

		public IEnumerable<string> SuccessfulUploadsFileNames { get; set; }
		public IEnumerable<string> FailedUploadsFileNames { get; set; }

	}
}
