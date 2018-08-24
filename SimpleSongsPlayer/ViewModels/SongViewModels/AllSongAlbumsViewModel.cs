using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class AllSongAlbumsViewModel : SongViewModelBase
    {
        public AllSongAlbumsViewModel() : base(new AllSongAlbumsGroupsFactory())
        {
        }
    }
}