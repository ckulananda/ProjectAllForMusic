namespace ProjectAllForMusic.Model
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int UserID { get; set; }
        public string FeedbackText { get; set; }
        public DateTime DateSubmitted { get; set; } = DateTime.Now; // Default to current time
    }
}
