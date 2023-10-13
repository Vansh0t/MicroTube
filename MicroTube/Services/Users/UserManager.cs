using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.Users
{
    public class UserManager : IUserManager
    {
        private const string SP_GET = "dbo.AppUser_Get";

        private readonly IAppUserDataAccess _dataAccess;

        public UserManager(IAppUserDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public async Task<IServiceResult<AppUser>> GetUser(int id)
        {
            var user = await _dataAccess.Get(id);
            return ServiceResult<AppUser>.FromResultObject(user);
        }

        public async Task<IServiceResult<AppUser>> GetUser(string credential)
        {
            throw new NotImplementedException();
        }
    }
}
