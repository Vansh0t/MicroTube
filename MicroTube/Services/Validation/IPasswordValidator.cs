namespace MicroTube.Services.Validation
{
    public interface IPasswordValidator
    {
        public IServiceResult Validate(string password);
    }
}
