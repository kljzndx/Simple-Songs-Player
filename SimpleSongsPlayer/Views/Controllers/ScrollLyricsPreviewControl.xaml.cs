﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.Views.Controllers.Abstracts;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class ScrollLyricsPreviewControl : LyricsPreviewControlBase
    {
        public ScrollLyricsPreviewControl()
        {
            this.InitializeComponent();
            base.Refreshed += ScrollLyricsPreview_Refreshed;
        }

        public event ItemClickEventHandler ItemClick;

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

        private void ScrollLyricsPreview_Refreshed(LyricsPreviewControlBase sender, LyricLine args)
        {
            if (args.Equals(LyricLine.Empty))
            {
                Main_ListView.SelectedIndex = -1;
                return;
            }

            Main_ListView.SelectedItem = args;
        }

        private void Main_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ItemClick?.Invoke(sender, e);
        }

        private async void Main_ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var args = e.AddedItems.Cast<LyricLine>().FirstOrDefault();
                foreach (var line in Source.Where(l => l.IsSelected))
                    line.IsSelected = false;

                if (args is null)
                {
                    Root_ScrollViewer.ChangeView(null, 0, null);
                    return;
                }

                args.IsSelected = true;
                Root_ScrollViewer.ChangeView(null, GetItemPosition(args), null);
            });
        }
    }
}
