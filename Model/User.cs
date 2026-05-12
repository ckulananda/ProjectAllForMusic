namespace ProjectAllForMusic.Model
{
    using System;

    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Musician, Artist, Instructor, Admin
        public string ProfilePicture { get; set; } // Path to profile picture
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
