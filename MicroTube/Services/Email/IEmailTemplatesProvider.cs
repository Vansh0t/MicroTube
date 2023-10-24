namespace MicroTube.Services.Email
{
    public interface IEmailTemplatesProvider
    {
        Task<string> BuildEmailConfirmationTemplate(string url);
        Task<string> BuildPasswordResetTemplate(string url);
    }
}