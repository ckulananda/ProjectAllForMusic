namespace ProjectAllForMusic.Model
{
    public class Instruments
    {
        public int InstrumentID { get; set; } // Primary Key
        public string InstrumentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Condition { get; set; } // New or Second-hand
        public decimal Price { get; set; }
        public int? SellerID { get; set; }
        public string? InstrumentPicture { get; set; } // Path to instrument image
        public DateTime DateAdded { get; set; } = DateTime.Now; // Default value
    }
}
