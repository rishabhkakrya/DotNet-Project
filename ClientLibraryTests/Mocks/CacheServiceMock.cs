using ClientLibrary.Services.Interfaces;
using ClientLibrary.Types;
using Moq;

namespace ClientLibrary.Tests.Mocks
{
    public class CacheServiceMock : Mock<ICacheService>
    {
        internal Dictionary<string, object> Cache = new();

        public CacheServiceMock() : base(MockBehavior.Strict)
        {
            Setup(_ => _.GetAsync<User>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, CancellationToken _) => Get<User>(key));

            Setup(_ => _.GetAsync<List<User>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, CancellationToken _) => Get<List<User>>(key));

            Setup(_ => _.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                .Callback((string key, object value, TimeSpan? _, CancellationToken _) => Set(key, value))
                .Returns(Task.CompletedTask);
        }

        private T? Get<T>(string key) where T : class
        {
            if (Cache.TryGetValue(key, out var value))
            {
                return value as T;
            }

            return null;
        }

        private void Set<T>(string key, T value) where T : class
        {
            Cache[key] = value;
        }
    }
}
