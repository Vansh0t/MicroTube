using Ardalis.GuardClauses;
using System.Text;

namespace MicroTube.Extensions
{
	public static class UrlExtensions
	{
		public static string UrlCombine(this string urlBase, params string[] urlRelative)
		{
			if(urlRelative.Length == 0)
			{
				return urlBase;
			}
			Guard.Against.NullOrWhiteSpace(urlBase);
			var resultBuilder = new StringBuilder(urlBase.TrimEnd('/'));
			foreach (var relative in urlRelative)
			{
				resultBuilder.Append($"/{relative.TrimStart('/')}");
			}
			return resultBuilder.ToString();
		}
	}
}
