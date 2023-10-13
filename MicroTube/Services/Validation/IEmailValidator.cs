namespace MicroTube.Services.Validation
{
    public interface IEmailValidator
    {
        public IServiceResult Validate(string email);
    }
}
