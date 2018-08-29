using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class SongArtistsViewModel : SongViewModelBase
    {
        public SongArtistsViewModel() : base(new AllSongArtistGroupsFactory())
        {
        }
    }
}