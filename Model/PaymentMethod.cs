namespace ProjectAllForMusic.Model
{
    public class PaymentMethod
    {
        public int PaymentMethodID { get; set; } // Primary Key
        public string MethodName { get; set; } = string.Empty; // e.g., Credit Card, PayPal, Bank Transfer
        public string Details { get; set; } = string.Empty; // Additional details for the payment method
    }
}
