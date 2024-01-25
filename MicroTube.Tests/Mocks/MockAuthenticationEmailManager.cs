using MicroTube.Services.Email;

namespace MicroTube.Tests.Mocks
{
    public class MockAuthenticationEmailManager : IAuthenticationEmailManager
    {
        public string? SentEmailChangeEnd { get; private set; }
        public string? SentEmailConfirmation { get; private set; }
        public string? SentPasswordResetStart { get; private set; }
        public string? SentPasswordResetEnd { get; private set; }
        public Task SendEmailChangeEnd(string recipient, string data)
        {
            SentEmailChangeEnd = data;
            return Task.CompletedTask;
        }
        public Task SendEmailConfirmation(string recipient, string data)
        {
            SentEmailConfirmation = data;
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEnd(string recipient, string data)
        {
            SentPasswordResetEnd = data;
            return Task.CompletedTask;
        }

        public Task SendPasswordResetStart(string recipient, string data)
        {
            SentPasswordResetStart = data;
            return Task.CompletedTask;
        }
    }
}
