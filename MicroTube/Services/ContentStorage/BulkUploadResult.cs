namespace MicroTube.Services.ContentStorage
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
