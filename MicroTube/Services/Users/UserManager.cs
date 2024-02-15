using MicroTube.Data.Access;
using MicroTube.Data.Models;

namespace MicroTube.Services.Users
{
    public class UserManager : IUserManager
    {
        private readonly IAppUserDataAccess _dataAccess;

        public UserManager(IAppUserDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

		public Task<IServiceResult<AppUser>> GetUserByCredential(string credential)
		{
			throw new NotImplementedException();
		}

		public async Task<IServiceResult<AppUser>> GetUserById(string id)
        {
            var user = await _dataAccess.Get(id);
            return ServiceResult<AppUser>.FromResultObject(user);
        }
    }
}
