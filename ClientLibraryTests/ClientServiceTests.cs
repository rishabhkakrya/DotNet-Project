using AutoFixture;
using ClientLibrary.Model;
using ClientLibrary.Services;
using ClientLibrary.Tests.Mocks;
using ClientLibrary.Types;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;

namespace ClientLibrary.Tests
{
    [TestFixture(Category = "ClientServiceTests")]
    internal class ClientServiceTests
    {
        private readonly Fixture _fixture = new();
        private HttpClientFactoryMock _httpClientFactoryMock;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _httpClientFactoryMock = new();

            var inMemorySettings = new Dictionary<string, string> {
                {"AppSettings:BaseUrl", _fixture.Create<string>()},
                {"AppSettings:ApiKey", _fixture.Create<string>()}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Test]
        public async Task GetUsersAsync_return_users()
        {
            // Arrange
            var usersMock = _fixture.CreateMany<User>(9).ToList();
            var responseMockPage1 = _fixture.Build<MultiPageResponse>()
                .With(_ => _.TotalPage, 2)
                .With(_ => _.Page, 1)
                .With(_ => _.Data, usersMock.Take(5).Cast<object>().ToArray().ToArray())
                .Create();

            var responseMockPage2 = _fixture.Build<MultiPageResponse>()
                .With(_ => _.TotalPage, 2)
                .With(_ => _.Page, 2)
                .With(_ => _.Data, usersMock.TakeLast(4).Cast<object>().ToArray())
                .Create();

            _httpClientFactoryMock.ResponseContentPage1 = JsonSerializer.Serialize(responseMockPage1);
            _httpClientFactoryMock.ResponseContentPage2 = JsonSerializer.Serialize(responseMockPage2);
            var clientService = new ClientService(_httpClientFactoryMock.Object, _configuration);

            // Act
            var users = await clientService.GetUsersAsync();

            // Assert
            Assert.IsNotNull(users);
            Assert.IsNotEmpty(users);
            Assert.That(users.Count, Is.EqualTo(usersMock.Count));
        }

        [Test]
        public async Task GetUsersAsync_return_user()
        {
            // Arrange
            var userMock = _fixture.Create<User>();
            var userDetailsResponse = new GetUserDetailsResponse
            {
                Data = userMock
            };

            _httpClientFactoryMock.ResponseContentPage1 = JsonSerializer.Serialize(userDetailsResponse);
            var clientService = new ClientService(_httpClientFactoryMock.Object, _configuration);

            // Act
            var user = await clientService.GetUserDetailsAsync(new GetUserDetailsRequest { Id = userMock.Id });

            // Assert
            Assert.IsNotNull(user);
            Assert.That(userMock.Id, Is.EqualTo(user.Id));
        }

        [Test]
        public void GetUsersAsync_throws_key_not_found_exception_for_not_found()
        {
            // Arrange
            var userMock = _fixture.Create<User>();
            _httpClientFactoryMock.ResponseStatusCode = HttpStatusCode.NotFound;
            var clientService = new ClientService(_httpClientFactoryMock.Object, _configuration);

            // Act and Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => clientService.GetUserDetailsAsync(new GetUserDetailsRequest { Id = userMock.Id }));
        }

        [Test]
        public void GetUsersAsync_throws_client_api_exception_for_internal_server_error()
        {
            // Arrange
            var userMock = _fixture.Create<User>();
            _httpClientFactoryMock.ResponseStatusCode = HttpStatusCode.InternalServerError;
            var clientService = new ClientService(_httpClientFactoryMock.Object, _configuration);

            // Act and Assert
            Assert.ThrowsAsync<ClientApiException>(() => clientService.GetUserDetailsAsync(new GetUserDetailsRequest { Id = userMock.Id }));
        }

        [Test]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.GatewayTimeout)] 
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.RequestTimeout)]
        [TestCase(HttpStatusCode.TooManyRequests)]
        public void GetUsersAsync_throws_timeout_exception_if_retry_fails(HttpStatusCode statusCode)
        {
            // Arrange
            var userMock = _fixture.Create<User>();
            _httpClientFactoryMock.ResponseStatusCode = statusCode;
            var clientService = new ClientService(_httpClientFactoryMock.Object, _configuration);

            // Act and Assert
            var ex = Assert.ThrowsAsync<TimeoutException>(() => clientService.GetUserDetailsAsync(new GetUserDetailsRequest { Id = userMock.Id }));
            Assert.That(ex?.Message, Does.Contain("3 retries"));
        }
    }
}
