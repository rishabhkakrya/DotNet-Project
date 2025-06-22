using ClientLibrary.Model;
using ClientLibrary.Services.Interfaces;
using ClientLibrary.Types;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace ClientLibrary.Services
{
    public class ClientService : IClientService
    {
        private const int NoOfRetries = 3;

        private const string RetryAttempt = "RetryAttempt";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _baseUrl;
        private readonly string? _apiKey;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ClientService(IHttpClientFactory? httpClientFactory = null, IConfiguration? configuration = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException($"Cannot find http client factory");
            _baseUrl = configuration?["AppSettings:BaseUrl"] ?? throw new ArgumentNullException($"Cannot find base url");
            _apiKey = configuration["AppSettings:ApiKey"] ?? throw new ArgumentNullException($"Cannot find api key");

            _retryPolicy = Policy
                   .Handle<ToBeRetriedException>(re => re.RetryAfterInSeconds > TimeSpan.Zero)
                   .WaitAndRetryAsync(
                       retryCount: NoOfRetries,
                       sleepDurationProvider: (_, ex, _) =>
                       {
                           var re = (ToBeRetriedException)ex;
                           return re.RetryAfterInSeconds;
                       },
                       onRetryAsync: (_, _, retryAttempt, context) =>
                       {
                           context[RetryAttempt] = retryAttempt;
                           return Task.CompletedTask;
                       });
        }

        public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var uri = $"{_baseUrl}users";

            var response = await GetAsync<List<User>>(uri, cancellationToken).ConfigureAwait(false);
            return response ?? new List<User>();
        }

        public async Task<User?> GetUserDetailsAsync(GetUserDetailsRequest request, CancellationToken cancellationToken = default)
        {
            var uri = $"{_baseUrl}users/{request.Id}";

            var response = await GetAsync<GetUserDetailsResponse>(uri, cancellationToken).ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
        {
            var currentPage = 1;
            using var httpClient = _httpClientFactory.CreateClient();
            using var response = await GetResponseAsync(httpClient, uri, currentPage, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            var isList = typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>);
            if (isList)
            {
                var multiPageResponse = JsonSerializer.Deserialize<MultiPageResponse>(json);
                json = JsonSerializer.Serialize(multiPageResponse?.Data);
                while (currentPage < multiPageResponse?.TotalPage)
                {
                    json = await AppendNextPageResultsAsync(httpClient, json, ++currentPage, uri, cancellationToken);
                }
            }

            var obj = JsonSerializer.Deserialize<T>(json);
            return obj;
        }

        private async Task<HttpResponseMessage> GetResponseAsync(HttpClient httpClient, string uri, int pageNumber, CancellationToken cancellationToken)
        {
            var builder = new UriBuilder(uri);
            var queryParams = HttpUtility.ParseQueryString(builder.Query);
            queryParams["page"] = pageNumber.ToString();
            builder.Query = queryParams.ToString();
            return await SendAsync(httpClient, HttpMethod.Get, builder.Uri.ToString(), cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> AppendNextPageResultsAsync(HttpClient httpClient, string json, int pageNumber, string uri, CancellationToken cancellationToken)
        {
            var response = await GetResponseAsync(httpClient, uri, pageNumber, cancellationToken);
            var pageJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var multiPageResponse = JsonSerializer.Deserialize<MultiPageResponse>(pageJson);
            pageJson = JsonSerializer.Serialize(multiPageResponse?.Data);
            var before = json.Trim().TrimEnd(new[] { ']' });
            var after = pageJson.Trim().TrimStart(new[] { '[' });
            return (after.Length > 1) ? $"{before},{after}" : json;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpMethod method, string uri, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async (context, _) =>
                {
                    using var request = GetRequestMessage(method, uri);
                    var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return response;

                    if (response.StatusCode is HttpStatusCode.NotFound)
                        throw new KeyNotFoundException($"Resource not found at {uri}");

                    if (response.StatusCode is HttpStatusCode.ServiceUnavailable ||
                        response.StatusCode is HttpStatusCode.GatewayTimeout
                        || response.StatusCode is HttpStatusCode.BadGateway ||
                        response.StatusCode is HttpStatusCode.RequestTimeout
                        || response.StatusCode is HttpStatusCode.TooManyRequests)
                    {
                        var retryAttempt = (context.TryGetValue(RetryAttempt, out var retryObject) && retryObject is int count) ? count : 0;
                        throw new ToBeRetriedException() { RetryAfterInSeconds = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) };
                    }

                    throw new ClientApiException($"Error in request {uri}");

                }, new Context(), cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (ToBeRetriedException)
            {
                throw new TimeoutException(
                    $"Could not get a valid response for Get request {uri} after {NoOfRetries} retries");
            }
        }

        private HttpRequestMessage GetRequestMessage(HttpMethod method, string uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }
    }
}
