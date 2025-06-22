using AutoFixture;
using ClientLibrary.Model;
using ClientLibrary.Services;
using ClientLibrary.Tests.Mocks;
using ClientLibrary.Types;

namespace ClientLibrary.Tests
{
    [TestFixture(Category = "ExternalUSerService")]
    internal class ExternalUserServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly CacheServiceMock _cacheServiceMock = new();
        private readonly ClientServiceMock _clientServiceMock = new();

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnUsers_From_API_And_Updates_Cache()
        {
            // Arrange
            var usersMock = _fixture.CreateMany<User>(5).ToList();
            usersMock.ForEach(u => _clientServiceMock.AddUser(u));

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act
            var users = await externalUserService.GetAllUsersAsync();

            // Assert
            Assert.IsNotNull(users);
            Assert.IsNotEmpty(users);
            Assert.AreEqual(usersMock.Count, users.Count);
            Assert.That(_cacheServiceMock.Cache.ContainsKey("AllUsers"), Is.True);
            
            var cachedUsers = _cacheServiceMock.Cache["AllUsers"] as List<User>;
            Assert.That(cachedUsers.Count, Is.EqualTo(users.Count));
        }


        [Test]
        public async Task GetAllUsersAsync_ShouldReturnUsers_From_Cache()
        {
            // Arrange
            var usersMock = _fixture.CreateMany<User>(5).ToList();
            _cacheServiceMock.Cache["AllUsers"] = usersMock;

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act
            var users = await externalUserService.GetAllUsersAsync();

            // Assert
            Assert.IsNotNull(users);
            Assert.IsNotEmpty(users);
            Assert.AreEqual(usersMock.Count, users.Count);
        }

        [Test]
        public void GetAllUsersAsync_Should_Throw_For_Failure()
        {
            // Arrange
            _clientServiceMock.IsFailure = true;

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act and Assert
            var ex = Assert.ThrowsAsync<Exception>(() => externalUserService.GetAllUsersAsync());
            Assert.That(ex?.InnerException, Is.InstanceOf<ClientApiException>());
        }

        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUsers_From_API_And_Updates_Cache()
        {
            // Arrange
            var usersMock = _fixture.CreateMany<User>(5).ToList();
            usersMock.ForEach(u => _clientServiceMock.AddUser(u));

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act
            var user = await externalUserService.GetUserByIdAsync(usersMock[1].Id);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(usersMock[1].Id, user.Id);
            Assert.AreEqual(usersMock[1].FirstName, user.FirstName);
            Assert.AreEqual(usersMock[1].LastName, user.LastName);
            Assert.That(_cacheServiceMock.Cache.ContainsKey($"User:{user.Id}"), Is.True);
            var cachesUser = _cacheServiceMock.Cache[$"User:{user.Id}"] as User;
            Assert.IsNotNull(cachesUser);
            Assert.AreEqual(user.Id, cachesUser.Id);
        }


        [Test]
        public async Task GetUserByIdAsync_ShouldReturnUsers_From_Cache()
        {
            // Arrange
            var userMock = _fixture.Create<User>();
            _cacheServiceMock.Cache[$"User:{userMock.Id}"] = userMock;

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act
            var user = await externalUserService.GetUserByIdAsync(userMock.Id);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(userMock.Id, user.Id);
        }

        [Test]
        public void GetUserByIdAsync_Should_Throw_For_Failure()
        {
            // Arrange
            _clientServiceMock.IsFailure = true;

            var externalUserService = new ExternalUserService(_clientServiceMock.Object, _cacheServiceMock.Object);

            // Act and Assert
            var ex = Assert.ThrowsAsync<Exception>(() => externalUserService.GetUserByIdAsync(_fixture.Create<int>()));
            Assert.That(ex?.InnerException, Is.InstanceOf<ClientApiException>());
        }
    }
}
