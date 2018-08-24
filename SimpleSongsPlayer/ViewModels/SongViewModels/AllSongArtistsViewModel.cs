using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public class AllSongArtistsViewModel : SongViewModelBase
    {
        public AllSongArtistsViewModel() : base(new AllSongArtistGroupsFactory())
        {
        }
    }
}