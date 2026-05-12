namespace ProjectAllForMusic.Model
{
    public class Transaction
    {
        public int TransactionID { get; set; } // Primary Key
        public int BuyerID { get; set; }
        public string ItemType { get; set; } = string.Empty; // Instrument, Lyric, LearningPackage
        public int ItemID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Payment method used (e.g., Credit Card, PayPal)
        public DateTime DatePurchased { get; set; } = DateTime.Now; // Default to current date and time
    }
}
