using MicroTube.Data.Models;
using MicroTube.Services;
using MicroTube.Services.Authentication;
using MicroTube.Services.Cryptography;

namespace MicroTube
{
    public static class Extensions
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
	}
}
