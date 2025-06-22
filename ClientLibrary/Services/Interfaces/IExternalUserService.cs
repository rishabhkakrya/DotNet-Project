using ClientLibrary.Types;

namespace ClientLibrary.Services.Interfaces
{
    public interface IExternalUserService
    {
        public Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

        public Task<List<User>?> GetAllUsersAsync(CancellationToken cancellationToken = default);
    }
}