namespace ProjectAllForMusic.Model
{
    public class MusicLyrics
    {
        public int LyricID { get; set; } // Primary Key
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? AuthorID { get; set; }
        public decimal? Price { get; set; }
        public string? FilePath { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now; // Default value
    }
}
