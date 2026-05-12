namespace ProjectAllForMusic.Model
{
    public class Request
    {
        public int RequestID { get; set; } // Primary Key
        public string RequestType { get; set; } = string.Empty; // Lesson, Instructor, Artist
        public int RequesterID { get; set; }
        public int RequestedEntityID { get; set; }
        public string Status { get; set; } = "Pending"; // Default status
        public DateTime DateRequested { get; set; } = DateTime.Now; // Default to current date and time
        public string RequestBody { get; set; }
    }
}
