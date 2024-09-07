using Azure.Storage.Blobs;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using MicroTube.Data.Access.Elasticsearch;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.ConfigOptions;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Search;
using MicroTube.Services.Authentication.BasicFlow;
using MicroTube.Services.VideoContent.Reactions;
using MicroTube.Services.VideoContent.Likes;
using Elastic.Transport.Products.Elasticsearch;
using MicroTube.Services.Validation;
using System.IO;
using System.IO.Abstractions;

namespace MicroTube
{
    //TODO: Needs refactoring.
    public static class GlobalExtensions
    {
        private const string DEFAULT_CONNECTION_STRING_NAME = "ConnectionStrings:Default";
        public static string GetDefaultConnectionString(this IConfiguration configuration)
        {
            string? defaultConnectionString = configuration[DEFAULT_CONNECTION_STRING_NAME];
            if (defaultConnectionString is null)
                throw new ConfigurationException("Configuration does not contain connection string " + DEFAULT_CONNECTION_STRING_NAME);
            return defaultConnectionString;
        }
        public static T GetRequiredByKey<T>(this IConfiguration configuration, string sectionKey)
        {
            T? result = configuration.GetRequiredSection(sectionKey).Get<T>();
            if (result == null)
                throw new ConfigurationException($"Unable to bind configuration section {sectionKey} to {typeof(T)}");
            return result;
        }
		public static string GetRequiredValue(this IConfiguration configuration, string path)
		{
			string? value = configuration[path];
			if(value == null)
				throw new ConfigurationException($"Required value is null. Path: {path}");
			return value;
		}
		public static string GetAppRootPath(this IConfiguration configuration)
		{
			string? value = configuration.GetValue<string>(WebHostDefaults.ContentRootKey);
			if (value == null)
				throw new ConfigurationException($"WebHostDefaults.ContentRootKey value is null.");
			return value;
		}
		public static IServiceResult<string> BuildJWTAccessToken(this IJwtTokenProvider tokenProvider, IJwtClaims claims, AppUser user)
		{
			return tokenProvider.GetToken(claims.GetClaims(user));
		}
		public static T GetRequiredObject<T>(this IServiceResult<T> result)
		{
			if (result.IsError)
				throw new RequiredObjectNotFoundException($"{nameof(IServiceResult<T>)} for must not be and error.");
			if (result.ResultObject == null)
				throw new RequiredObjectNotFoundException($"A successful {nameof(IServiceResult<T>)} must always provide ResultObject.");
			return result.ResultObject;
		}
		public static void AddRefreshTokenCookie(this HttpContext context, IConfiguration config, string refreshToken, DateTime expiration)
		{
			string path = config.GetRequiredByKey<string>("UserSession:RefreshPath");
			string domain = config.GetRequiredValue("ClientApp:Domain");
			CookieOptions options = new CookieOptions
			{
				Expires = expiration,
				Domain = domain,
				Path = path,
				HttpOnly = true,
				SameSite = SameSiteMode.None,
				Secure = true
			};
			context.Response.Cookies.Append(Constants.AuthorizationConstants.REFRESH_TOKEN_COOKIE_KEY, refreshToken, options);
		}

		public static T GetRequired<T>(this IConfigurationSection section) where T : class
		{
			T? result = section.Get<T>();
			if (result == null)
			{
				throw new ConfigurationException($"Unable to find and map a required configuration section of type {typeof(T)}");
			}
			return result;
		}
		
		
		public static IServiceResult ExceptionToErrorResult(this Exception exception, string prependInfo = "", int code = 500)
		{
			var result = ServiceResult.Fail(code, $"{prependInfo}. {exception}");
			return result;
		}
		public static IServiceResult<T> ExceptionToErrorResult<T>(
			this Exception exception,
			string prependInfo = "",
			int code = 500)
		{
			var result = ServiceResult<T>.Fail(code, $"{prependInfo}. {exception}");
			return result;
		}
		
		
		public static string? GetIp(this HttpContext context, bool bypassProxy = true)
		{
			if (bypassProxy && context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp))
			{
				return forwardedIp.ToString();
			}
			return context.Connection.RemoteIpAddress?.ToString();
		}
		public static bool TryDeleteFileOrDirectory(this IFileSystem fileSystem, string path)
		{
			try
			{
				if (fileSystem.File.Exists(path))
				{
					fileSystem.File.Delete(path);
					return !File.Exists(path);
				}
				if (fileSystem.Directory.Exists(path))
				{
					fileSystem.Directory.Delete(path, true);
					return !fileSystem.Directory.Exists(path);
				}
			}
			catch (Exception e)
			{
				return false;
			}
			return false;
		}
		
	}
}
