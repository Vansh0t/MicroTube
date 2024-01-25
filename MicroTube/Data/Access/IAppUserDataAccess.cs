using MicroTube.Data.Models;

namespace MicroTube.Data.Access
{
    public interface IAppUserDataAccess
    {
        Task<AppUser?> Get(int id);
        Task<AppUser?> GetByEmail(string email);
        Task<AppUser?> GetByUsername(string username);
    }
}