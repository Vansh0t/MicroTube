using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IAuthenticationDataAccess<TAuthModel>
    {
        public Task<int> CreateUser(string username, string email, TAuthModel auth);
        public Task<TAuthModel?> Get(int userId);
        public Task UpdateEmailConfirmation(EmailPasswordAuthentication auth);
        public Task UpdatePasswordReset(EmailPasswordAuthentication auth);
        public Task UpdateEmailAndConfirmation(EmailPasswordAuthentication auth, string newEmail);
        public Task UpdatePasswordHashAndReset(EmailPasswordAuthentication auth, string passwordHash);
    }
}
