using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.DAL
{
    public class LyricFile : ILibraryFile
    {
        public string FileName { get; set; }
        public string LibraryFolder { get; set; }
        [Key]
        public string Path { get; set; }
        [Required(AllowEmptyStrings = false)]
        public DateTime ChangeDate { get; set; }
    }
}