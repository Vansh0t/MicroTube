using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IAuthenticationDataAccess<TAuthModel>
    {
        public Task<int> CreateUser(string username, string email, TAuthModel auth);
        public Task<TAuthModel?> Get(int userId);
    }
}
