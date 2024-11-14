namespace MicroTube.Services.ConfigOptions
{
	public class VideoCommentSearchOptions
	{
		public const string KEY = "VideoCommentSearch";
		public int PaginationMaxBatchSize { get; set; }

		public VideoCommentSearchOptions(int paginationMaxBatchSize)
		{
			PaginationMaxBatchSize = paginationMaxBatchSize;
		}
	}
}
