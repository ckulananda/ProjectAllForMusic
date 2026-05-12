namespace ProjectAllForMusic.Model
{
    public class Response
    {
        public object Data { get; set; } // Holds the response data
        public int StatusCode { get; set; } // Holds the HTTP status code
        public string StatusMessage { get; set; } // Holds the status message
    }
}
