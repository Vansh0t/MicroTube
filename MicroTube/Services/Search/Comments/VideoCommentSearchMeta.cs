namespace MicroTube.Services.Search.Comments
{
	public class VideoCommentSearchMeta
	{

		public DateTime LastTime { get; set; }
		public int LastLikes { get; set; }
		public string LastId { get; set; }
		public int LastDislikes { get; set; }
		public VideoCommentSearchMeta(DateTime lastTime, int lastLikes, int lastDislikes, string lastId)
		{
			LastTime = lastTime;
			LastLikes = lastLikes;
			LastDislikes = lastDislikes;
			LastId = lastId;
		}

	}
}
