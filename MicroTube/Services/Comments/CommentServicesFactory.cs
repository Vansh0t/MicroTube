using MicroTube.Constants;
using MicroTube.Services.Comments.Reactions;
using MicroTube.Services.VideoContent.Comments;
using System.Diagnostics.CodeAnalysis;

namespace MicroTube.Services.Comments
{
	public class CommentServicesFactory
	{
		private IEnumerable<ICommentingService> _commentingServices;
		private IEnumerable<ICommentReactionsProvider> _commentReactionsProviderServices;
		private readonly Dictionary<string, Type> _commentingServiceMap= new Dictionary<string, Type>()
		{
			{FactoryConstants.TARGET_VIDEO_KEY, typeof(DefaultVideoCommentingService) }
			//add additional services for different comment targets (articles, subcomments, etc.)
		};
		private readonly Dictionary<string, Type> _commentReactionsProviderServiceMap = new Dictionary<string, Type>()
		{
			{FactoryConstants.TARGET_VIDEO_KEY, typeof(LikeDislikeVideoCommentReactionsProvider) }
			//add additional services for different comment targets (articles, subcomments, etc.)
		};
		public CommentServicesFactory(IEnumerable<ICommentingService> commentingServices, IEnumerable<ICommentReactionsProvider> reactionsProviderServices)
		{
			_commentingServices = commentingServices.ToArray();
			_commentReactionsProviderServices = reactionsProviderServices.ToArray();
		}

		public bool TryGetCommentingService(string key, [NotNullWhen(returnValue: true)] out ICommentingService? service)
		{
			service = null;
			if(!_commentingServiceMap.TryGetValue(key, out var serviceType))
			{
				return false;
			}
			service = _commentingServices.FirstOrDefault(_ => _.GetType() == serviceType);
			return service != null;
		}
		public bool TryGetCommentReactionsProviderService(string key, [NotNullWhen(returnValue: true)] out ICommentReactionsProvider? service)
		{
			service = null;
			if (!_commentingServiceMap.TryGetValue(key, out var serviceType))
			{
				return false;
			}
			service = _commentReactionsProviderServices.FirstOrDefault(_ => _.GetType() == serviceType);
			return service != null;
		}
	}
}
