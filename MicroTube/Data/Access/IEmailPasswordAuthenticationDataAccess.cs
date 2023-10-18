using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IEmailPasswordAuthenticationDataAccess : IAuthenticationDataAccess<EmailPasswordAuthentication>
    {
        public Task<EmailPasswordAuthentication?> GetByEmailVerificationString(string emailVerificationString);
        public Task<EmailPasswordAuthentication?> GetByPasswordResetString(string passwordResetString);
        public Task<EmailPasswordAppUser?> GetWithUser(int userId);
        public Task<EmailPasswordAppUser?> GetWithUserByCredential(string credential);
        public Task UpdateEmailConfirmation(EmailPasswordAuthentication auth, bool isEmailConfirmed);
        public Task UpdatePasswordReset(EmailPasswordAuthentication auth);
        public Task UpdateEmailAndConfirmation(EmailPasswordAuthentication auth, string newEmail, bool isEmailConfirmed);
        public Task UpdatePasswordHashAndReset(EmailPasswordAuthentication auth, string passwordHash);
    }
}
