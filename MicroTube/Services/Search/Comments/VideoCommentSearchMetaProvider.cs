using MicroTube.Data.Models.Comments;
using System.Text.Json;

namespace MicroTube.Services.Search.Comments
{
	public class VideoCommentSearchMetaProvider : ISearchMetaProvider<IEnumerable<VideoComment>, VideoCommentSearchMeta>
	{
		public VideoCommentSearchMeta? BuildMeta(IEnumerable<VideoComment> data)
		{
			VideoComment? last = data.LastOrDefault();
			if(last == null)
			{
				return null;
			}
			if(last.Reactions == null)
			{
				throw new RequiredObjectNotFoundException("Unable to build search meta: comment reactions is null");
			}
			return new VideoCommentSearchMeta(last.Time, last.Reactions.Likes, last.Reactions.Dislikes, last.Id.ToString());
		}

		public VideoCommentSearchMeta? DeserializeMeta(string? data)
		{
			if(string.IsNullOrWhiteSpace(data))
			{
				return null;
			}
			return JsonSerializer.Deserialize<VideoCommentSearchMeta>(data);
		}

		public string SerializeMeta(VideoCommentSearchMeta data)
		{
			return JsonSerializer.Serialize(data);
		}
	}
}
