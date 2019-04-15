using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSongsPlayer.DAL
{
    public enum FileSourceMembers
    {
        MusicLibrary,
        Other
    }

    public class PlaybackItem
    {
        private static ulong AllItemCount = 0;

        private ulong index;

        public PlaybackItem()
        {

        }

        public PlaybackItem(string path, FileSourceMembers source)
        {
            Index = AllItemCount + 1;
            Path = path;
            Source = source;
        }
        
        public ulong Index
        {
            get => index;
            set
            {
                index = value;

                if (value > AllItemCount)
                    AllItemCount = value;
            }
        }

        [Key]
        public string Path { get; set; }

        public FileSourceMembers Source { get; set; }

        public static void ResetCount()
        {
            AllItemCount = 0;
        }
    }
}