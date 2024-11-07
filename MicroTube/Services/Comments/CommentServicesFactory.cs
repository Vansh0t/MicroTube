using MicroTube.Constants;
using MicroTube.Services.VideoContent.Comments;

namespace MicroTube.Services.Comments
{
	public class CommentServicesFactory
	{
		private IEnumerable<ICommentingService> _commentingServices;
		private readonly Dictionary<string, Type> _serviceMap = new Dictionary<string, Type>()
		{
			{FactoryConstants.TARGET_VIDEO_KEY, typeof(DefaultVideoCommentingService) }
			//add additional services for different comment targets (articles, subcomments, etc.)
		};
		public CommentServicesFactory(IEnumerable<ICommentingService> commentingServices)
		{
			_commentingServices = commentingServices.ToArray();
		}

		public bool TryGetCommentingService(string key, out ICommentingService? service)
		{
			service = null;
			if(!_serviceMap.TryGetValue(key, out var serviceType))
			{
				return false;
			}
			service = _commentingServices.FirstOrDefault(_ => _.GetType() == serviceType);
			return service != null;
		}
	}
}
