using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class SongAlbumsViewModel : SongViewModelBase
    {
        public SongAlbumsViewModel() : base(new AllSongAlbumsGroupsFactory())
        {
        }
    }
}