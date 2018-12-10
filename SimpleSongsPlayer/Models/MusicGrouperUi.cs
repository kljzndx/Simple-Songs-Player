using Windows.ApplicationModel.Resources;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

namespace SimpleSongsPlayer.Models
{
    public class MusicGrouperUi
    {
        public MusicGrouperUi(string resourceKey, IMusicGrouper grouper)
        {
            Name = StringResources.ListStringResource.GetString(resourceKey);
            Grouper = grouper;
        }

        public string Name { get; }
        public IMusicGrouper Grouper { get; }
    }
}