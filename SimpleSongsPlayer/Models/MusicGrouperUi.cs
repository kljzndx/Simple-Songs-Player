using Windows.ApplicationModel.Resources;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

namespace SimpleSongsPlayer.Models
{
    public class MusicGrouperUi
    {
        private static readonly ResourceLoader ListStringResource = ResourceLoader.GetForCurrentView("MusicListPage");

        public MusicGrouperUi(string resourceKey, IMusicGrouper grouper)
        {
            Name = ListStringResource.GetString(resourceKey);
            Grouper = grouper;
        }

        public string Name { get; }
        public IMusicGrouper Grouper { get; }
    }
}