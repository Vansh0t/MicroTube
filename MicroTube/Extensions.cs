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
	}
}
