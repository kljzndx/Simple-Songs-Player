using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels.Events;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class ScrollLyricsPreview : LyricsPreviewControlBase
    {
        public ScrollLyricsPreview()
        {
            this.InitializeComponent();
            base.Refreshed += ScrollLyricsPreview_Refreshed;
        }

        private double GetItemPosition(LyricLine line)
        {
            ListViewItem container = Main_ListView.ContainerFromItem(line) as ListViewItem;
            if (container == null)
                return 0;

            var transform = container.TransformToVisual(Main_ListView);

            Point position = transform.TransformPoint(new Point());
            double result = (position.Y) + (container.ActualHeight / 2D) - (Root_ScrollViewer.ActualHeight / 2D);

            return result > 0 ? result : 0;
        }

        private void ScrollLyricsPreview_Refreshed(LyricsPreviewControlBase sender, LyricsPreviewRefreshEventArgs args)
        {
            foreach (var line in Lyrics.Where(l => l.IsSelected))
                line.IsSelected = false;
           
            if (args.CurrentLyric.Equals(LyricLine.Empty))
            {
                Main_ListView.SelectedItem = null;
                Root_ScrollViewer.ChangeView(null, 0, null);
                return;
            }

            Main_ListView.SelectedItem = args.CurrentLyric;
            args.CurrentLyric.IsSelected = true;
            Root_ScrollViewer.ChangeView(null, GetItemPosition(args.CurrentLyric), null);
        }
    }
}
