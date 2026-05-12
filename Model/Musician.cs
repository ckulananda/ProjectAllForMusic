namespace ProjectAllForMusic.Model
{
    using System;

    public class Musician
    {
        public string MusicianID { get; set; } // Primary Key
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country { get; set; }
        public string ContactNumber { get; set; }
        public string Genre { get; set; }
    }
}
