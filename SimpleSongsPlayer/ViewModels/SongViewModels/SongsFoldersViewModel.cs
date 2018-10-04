using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class SongsFoldersViewModel : SongViewModelBase
    {
        public SongsFoldersViewModel() : base(new AllSongsFoldersFactory())
        {
        }
    }
}