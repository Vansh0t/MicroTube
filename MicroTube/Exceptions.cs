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
}
