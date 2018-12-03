using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.ViewModels
{
    public static class MusicPusher
    {
        private static void AddItem(MediaPlaybackList target, MediaPlaybackItem item)
        {
            if (!target.Items.Contains(item))
                target.Items.Add(item);
        }

        public static async Task Push(MusicFileDTO music)
        {
            var playbackItem = await music.GetPlaybackItem();

            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                if (mpl.Items.Contains(playbackItem))
                    mpl.Items.Remove(playbackItem);

                mpl.Items.Insert(0, playbackItem);
                mpl.MoveTo(0);
            }
            else
                Push(new[] {playbackItem});
        }

        public static async Task Push(IEnumerable<MusicFileDTO> items)
        {
            List<MediaPlaybackItem> list = new List<MediaPlaybackItem>();
            foreach (var file in items)
                list.Add(await file.GetPlaybackItem());

            Push(list);
        }

        public static async Task PushToNext(MusicFileDTO file)
        {
            var mpi = await file.GetPlaybackItem();

            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                if (mpl.CurrentItem.Equals(mpi))
                    return;

                if (mpl.Items.Contains(mpi))
                    mpl.Items.Remove(mpi);

                uint id = mpl.CurrentItemIndex + 1;
                mpl.Items.Insert((int) id, mpi);
            }
            else
                Push(new[] {mpi});
        }

        public static async Task Append(MusicFileDTO file) => await Append(new[] {file});

        public static async Task Append(IEnumerable<MusicFileDTO> files)
        {
            List<MediaPlaybackItem> items = new List<MediaPlaybackItem>();
            foreach (var file in files)
                items.Add(await file.GetPlaybackItem());

            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
                foreach (var item in items)
                    AddItem(mpl, item);
            else
                Push(items);
        }

        private static void Push(IEnumerable<MediaPlaybackItem> items)
        {
            var mpl = new MediaPlaybackList();
            foreach (var item in items)
                AddItem(mpl, item);
            App.MediaPlayer.Source = mpl;
        }
    }
}