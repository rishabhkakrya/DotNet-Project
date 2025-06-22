using ClientLibrary.Services.Interfaces;
using ClientLibrary.Types;

namespace ClientLibrary.Services
{
    public class ExternalUserService(IClientService clientService, ICacheService cacheService) : IExternalUserService
    {
        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userInfo = await cacheService.GetAsync<User>($"User:{userId}", cancellationToken).ConfigureAwait(false);
                if (userInfo != null)
                    return userInfo;

                userInfo = await clientService.GetUserDetailsAsync(new GetUserDetailsRequest { Id = userId }, cancellationToken).ConfigureAwait(false);
                
                if (userInfo != null)
                    await cacheService.SetAsync($"User:{userId}", userInfo, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);
                
                return userInfo;
            }
            catch (Exception e)
            {
                throw new Exception($"Error in getting user id: {userId}", e);
            }
        }

        public async Task<List<User>?> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var allUsers = await cacheService.GetAsync<List<User>>("AllUsers", cancellationToken).ConfigureAwait(false);
                if (allUsers != null)
                    return allUsers;

                allUsers = await clientService.GetUsersAsync(cancellationToken).ConfigureAwait(false);

                if (allUsers.Any())
                    await cacheService.SetAsync("AllUsers", allUsers, TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false);

                return allUsers;
            }
            catch (Exception e)
            {
                throw new Exception("Error in getting all users", e);
            }
        }
    }
}
