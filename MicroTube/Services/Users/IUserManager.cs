using MicroTube.Data.Models;

namespace MicroTube.Services.Users
{
    public interface IUserManager
    {
        public Task<IServiceResult<AppUser>> GetUserById(string id);
        public Task<IServiceResult<AppUser>> GetUserByCredential(string credential);
    }
}
