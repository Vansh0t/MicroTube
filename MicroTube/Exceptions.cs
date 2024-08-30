namespace MicroTube
{
    public class ConfigurationException:Exception
    {
        public ConfigurationException(string message):base(message)
        {
            
        }
    }
    public class DataAccessException : Exception
    {
        public DataAccessException(string message) : base(message)
        {

        }
    }
    public class RequiredObjectNotFoundException : Exception
    {
        public RequiredObjectNotFoundException(string message) : base(message)
        {

        }
    }
	public class BackgroundJobException: Exception
	{
		public BackgroundJobException(string? message, Exception? innerException): base(message, innerException)
		{

		}
		public BackgroundJobException(string? message): base(message)
		{

		}
	}
	public class ExternalServiceException: Exception
	{
		public ExternalServiceException(string? message, Exception? innerException) : base(message, innerException)
		{

		}
		public ExternalServiceException(string? message) : base(message)
		{

		}
	}
}
