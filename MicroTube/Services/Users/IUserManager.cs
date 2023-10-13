using MicroTube.Data.Models;

namespace MicroTube.Services.Users
{
    public interface IUserManager
    {
        public Task<IServiceResult<AppUser>> GetUser(int id);
        public Task<IServiceResult<AppUser>> GetUser(string credential);
    }
}
