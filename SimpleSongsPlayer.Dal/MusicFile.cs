using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SimpleSongsPlayer.Dal
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbDataAttribute : Attribute
    {
    }

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

        [DbData]
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

        [DbData]
        public bool IsInPlaybackList { get; set; }

        [DbData]
        public string DbVersion { get; set; }

        public void UpdateFileInfo(MusicFile newInfo)
        {
            var props = this.GetType().GetProperties();

            foreach (var prop in props)
            {
                var att = prop.GetCustomAttribute<DbDataAttribute>();

                if (att == null)
                    prop.SetValue(this, prop.GetValue(newInfo));
            }
        }
    }
}
