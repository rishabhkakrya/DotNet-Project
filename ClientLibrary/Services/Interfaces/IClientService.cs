using ClientLibrary.Types;

namespace ClientLibrary.Services.Interfaces
{
    public interface IClientService
    {
        public Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default);

        public Task<User?> GetUserDetailsAsync(GetUserDetailsRequest getUserDetailsRequest, CancellationToken cancellationToken = default);
    }
}
