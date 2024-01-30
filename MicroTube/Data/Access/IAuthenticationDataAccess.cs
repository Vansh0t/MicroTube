namespace MicroTube.Data.Access
{
    public interface IAuthenticationDataAccess<TAuthModel>
    {
        public Task<string?> CreateUser(string username, string email, TAuthModel auth);
        public Task<TAuthModel?> Get(string userId);
    }
}
