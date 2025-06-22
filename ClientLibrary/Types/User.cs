using System.Text.Json.Serialization;

namespace ClientLibrary.Types
{
    public class MultiPageResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        [JsonPropertyName("total_pages")]
        public int TotalPage { get; set; }

        [JsonPropertyName("data")]
        public object[] Data { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
    }

    public class GetUserDetailsRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class GetUserDetailsResponse
    {
        [JsonPropertyName("data")]
        public User? Data { get; set; }
    }
}