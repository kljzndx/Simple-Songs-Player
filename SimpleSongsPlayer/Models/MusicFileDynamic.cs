using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileDynamic : ObservableObject
    {
        private bool isSelected;

        public MusicFileDynamic(MusicFileDTO original)
        {
            Original = original;
        }

        public MusicFileDTO Original { get; }
        
        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }
    }
}