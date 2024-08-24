using Microsoft.Extensions.Configuration;
using System.Data;
using System.Net;
using System.Text.Json;

namespace MicroTube.Tests
{
    public static class Extensions
    {
		public static IEnumerable<Cookie> GetSetCookieKeyValues(this HttpResponseMessage response)
		{
			var responseCookies = response.Headers.GetValues("Set-Cookie");
			var cookies = responseCookies.Select(_ =>
			{
				var split = _.Split('=');
				return new Cookie(split[0].Trim(), split[1].Trim());
			});
			return cookies;
		}
		public static IConfigurationBuilder AddConfigObject<T>(this IConfigurationBuilder configBuilder, string key, T objectToAdd)
		{
			Dictionary<string, string?> serializedObject = new Dictionary<string, string?>()
			{
				{key, JsonSerializer.Serialize(objectToAdd)}
			};
			configBuilder.AddInMemoryCollection(serializedObject);
			return configBuilder;
		}
		/// <summary>
		/// A shortcut to check class equality by its content. Only public primitive and semi-primitive types are used for comparison
		/// Supported property types:
		/// <see cref="Type.IsPrimitive"/>,
		/// <see cref="string"/>,
		/// <see cref="decimal"/>,
		/// <see cref="DateTime"/>,
		/// <see cref="Guid"/>
		/// </summary>
		public static bool IsEqualByContentValues<T>(this T self, T other)
			where T: class
		{
			foreach (var prop in typeof(T).GetProperties())
			{
				var value1 = prop.GetValue(self);
				var value2 = prop.GetValue(other);
				Type propType = prop.PropertyType;
				if(!propType.IsPrimitive 
					&& propType != typeof(string) 
					&& propType != typeof(decimal) 
					&& propType != typeof(DateTime)
					&& propType != typeof(Guid))
				{
					continue;
				}
				if (value1 != null)
				{
					if(!value1.Equals(value2))
						return false;
				}
				else if(value2 != null)
				{
					if (!value2.Equals(value1))
						return false;
				}
			}
			return true;
		}
    }
}
