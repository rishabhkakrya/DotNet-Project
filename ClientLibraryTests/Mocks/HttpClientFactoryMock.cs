using Moq;
using Moq.Protected;
using System.Net;

namespace ClientLibrary.Tests.Mocks
{
    internal class HttpClientFactoryMock : Mock<IHttpClientFactory>
    {
        internal string ResponseContentPage1 = string.Empty;
        internal string ResponseContentPage2 = string.Empty;
        internal HttpStatusCode ResponseStatusCode = HttpStatusCode.OK;

        public HttpClientFactoryMock() : base(MockBehavior.Strict)
        {
            Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns((string _) => CreateClient());
        }

        private HttpClient CreateClient()
        {
            if (!string.IsNullOrEmpty(ResponseContentPage2))
            {
                return CreateMultiPageClient();
            }

            return CreateSinglePageClient();
        }

        private HttpClient CreateSinglePageClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = ResponseStatusCode,
                    Content = new StringContent(ResponseContentPage1)
                });

            return new HttpClient(handlerMock.Object);
        }

        private HttpClient CreateMultiPageClient()
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = ResponseStatusCode,
                    Content = new StringContent(ResponseContentPage1)
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = ResponseStatusCode,
                    Content = new StringContent(ResponseContentPage2)
                });

            return new HttpClient(handlerMock.Object);
        }
    }
}
