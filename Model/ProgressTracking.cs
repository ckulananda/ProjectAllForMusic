namespace ProjectAllForMusic.Model
{
    public class ProgressTracking
    {
        public int ProgressID { get; set; }
        public int UserID { get; set; }
        public string Details { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now; // Default to current time
    }
}
