using ClientLibrary.Model;
using ClientLibrary.Services.Interfaces;
using ClientLibrary.Types;
using Moq;

namespace ClientLibrary.Tests.Mocks
{
    public class ClientServiceMock : Mock<IClientService>
    {
        private readonly Dictionary<int, User> _users = new();
        internal bool IsFailure = false;

        public ClientServiceMock() : base(MockBehavior.Strict)
        {
            Setup(_ => _.GetUsersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((CancellationToken _) => GetUsersAsync());

            Setup(_ => _.GetUserDetailsAsync(It.IsAny<GetUserDetailsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetUserDetailsRequest userDetailsRequest, CancellationToken _) => GetUserDetailsAsync(userDetailsRequest));
        }

        private List<User> GetUsersAsync()
        {
            if (IsFailure)
            {
                IsFailure = false;
                throw new ClientApiException("Simulated failure in GetUsersAsync");
            }

            return _users.Values.ToList();
        }

        private User? GetUserDetailsAsync(GetUserDetailsRequest getUserDetailsRequest)
        {
            if (IsFailure)
            {
                IsFailure = false;
                throw new ClientApiException("Simulated failure in GetUserDetailsAsync");
            }

            return _users.GetValueOrDefault(getUserDetailsRequest.Id);
        }

        public void AddUser(User user)
        {
            if (user == null || _users.ContainsKey(user.Id))
                throw new ArgumentException("User cannot be null and must have a unique Id.");

            _users[user.Id] = user;
        }
    }
}
