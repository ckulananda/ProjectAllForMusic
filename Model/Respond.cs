namespace ProjectAllForMusic.Model
{
    public class Respond
    {
        public int ResponseID { get; set; }
        public int RequestID { get; set; }
        public int ResponderID { get; set; }
        public string RespondBody { get; set; }
        public DateTime DateResponded { get; set; } = DateTime.Now;
        public string RequesterID { get; set; } // Added the RequesterID property
    }
}
