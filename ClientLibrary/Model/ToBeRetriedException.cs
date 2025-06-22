using System.Net;

namespace ClientLibrary.Model
{
    internal class ToBeRetriedException : Exception
    {
        public TimeSpan RetryAfterInSeconds { get; set; }
    }
}
