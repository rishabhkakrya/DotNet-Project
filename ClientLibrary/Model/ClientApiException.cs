namespace ClientLibrary.Model
{
    public class ClientApiException : Exception
    {
        public ClientApiException(string message) : base(message)
        {
        }

        public ClientApiException() : base("An error occurred while processing the request.")
        {
        }
    }
}
