using MicroTube.Constants;
using MicroTube.Services.Comments.Reactions;
using MicroTube.Services.VideoContent.Likes;

namespace MicroTube.Services.Reactions
{
	public class ReactionServicesFactory
	{
		private IEnumerable<ILikeDislikeReactionService> _likeDislikeReactionServices;
		private readonly Dictionary<string, Type> _serviceMap = new Dictionary<string, Type>()
		{
			{FactoryConstants.TARGET_VIDEO_KEY, typeof(DefaultVideoReactionsService) },
			{FactoryConstants.TARGET_COMMENT_KEY, typeof(DefaultVideoCommentReactionsService) }
			//add additional services for different reaction targets (articles, subcomments, etc.)
		};
		public ReactionServicesFactory(IEnumerable<ILikeDislikeReactionService> commentingServices)
		{
			_likeDislikeReactionServices = commentingServices;
		}

		public bool TryGetLikeDislikeService(string key, out ILikeDislikeReactionService? service)
		{
			service = null;
			if (!_serviceMap.TryGetValue(key, out var serviceType))
			{
				return false;
			}
			service = _likeDislikeReactionServices.FirstOrDefault(_ => _.GetType() == serviceType);
			return service != null;
		}
	}
}
