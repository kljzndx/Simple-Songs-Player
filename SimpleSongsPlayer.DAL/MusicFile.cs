using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.DAL
{
    public class MusicFile : ILibraryFile
    {
        public string FileName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "库文件夹名称为空")]
        public string LibraryFolder { get; set; }
        public string ParentFolder { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "文件路径为空")]
        [Key]
        public string Path { get; set; }
        public DateTime ChangeDate { get; set; }

        public uint TrackNumber { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "音乐标题为空")]
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "音乐持续时间为空")]
        public TimeSpan Duration { get; set; }

        public string DBVersion { get; set; }
    }
}
