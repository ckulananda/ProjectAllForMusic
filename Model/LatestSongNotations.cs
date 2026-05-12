using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectAllForMusic.Model
{
    public class LatestSongNotations
    {
       
        public int NotationID { get; set; }

        
        public string SongTitle { get; set; }

        public string ArtistName { get; set; } // Optional but useful for filtering

        public string Genre { get; set; } // Genre-based search

        public string DifficultyLevel { get; set; } // Easy, Medium, Hard

        
        public string Notation { get; set; }

        public DateTime DateAdded { get;set; } = DateTime.Now; // Ensures no manual changes
    }
}
