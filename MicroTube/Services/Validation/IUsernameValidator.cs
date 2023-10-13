namespace MicroTube.Services.Validation
{
    public interface IUsernameValidator
    {
        public IServiceResult Validate(string username);
    }
}
