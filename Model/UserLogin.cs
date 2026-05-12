namespace ProjectAllForMusic.Model
{
    public class UserLogin
    {
        public string Email { get; set; }  // Email is used for login
        public string Password { get; set; }  // Password input from user (should be hashed before checking)
    }

}