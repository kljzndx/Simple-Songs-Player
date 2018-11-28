using System;

namespace SimpleSongsPlayer.ViewModels.Factories.MusicFilters
{
    public class MusicFilterArgs
    {
        public MusicFilterArgs(IMusicFilter filter, object args)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            Args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IMusicFilter Filter { get; }
        public object Args { get; }
    }
}