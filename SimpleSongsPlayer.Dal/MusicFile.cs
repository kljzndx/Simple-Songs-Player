using System;
using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.Dal
{
    public class MusicFile
    {
        public MusicFile() { }

        public MusicFile(string title, string artist, string album, uint trackNumber, TimeSpan duration
                        , string filePath, string libraryFolder, DateTime fileChangeDate)
        {
            Title = title;
            Artist = artist;
            Album = album;
            TrackNumber = trackNumber;
            Duration = duration;

            FilePath = filePath;
            LibraryFolder = libraryFolder;
            FileChangeDate = fileChangeDate;
        }

        [Key]
        public int Index { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public uint TrackNumber { get; set; }
        public TimeSpan Duration { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string FilePath { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LibraryFolder { get; set; }
        public DateTime FileChangeDate { get; set; }

        public bool IsInPlaybackList { get; set; }

        public string DbVersion { get; set; }
    }
}
