namespace ProjectAllForMusic.Model
{
    using System;

    public class LearningPackage
    {
        public int PackageID { get; set; }
        public string LearningPackageName { get; set; }
        public int InstructorID { get; set; }
        public string InstructorName { get; set; }
        public string LearningMaterials { get; set; } // Stores file paths (Doc, PDF, RAR, etc.)
        public string Videos { get; set; } // Stores file paths for videos (any format)
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
