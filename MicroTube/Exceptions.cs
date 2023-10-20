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
}
