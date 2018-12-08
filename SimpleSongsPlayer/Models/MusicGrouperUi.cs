using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

namespace SimpleSongsPlayer.Models
{
    public class MusicGrouperUi
    {
        public MusicGrouperUi(string name, IMusicGrouper grouper)
        {
            Name = name;
            Grouper = grouper;
        }

        public string Name { get; }
        public IMusicGrouper Grouper { get; }
    }
}