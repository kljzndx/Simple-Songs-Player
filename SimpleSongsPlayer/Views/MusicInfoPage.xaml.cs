using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.Views.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicInfoPage : Page
    {
        private MusicInfoViewModel _vm;

        private bool _isShort;

        public MusicInfoPage()
        {
            this.InitializeComponent();
            _vm = Ioc.Default.GetRequiredService<MusicInfoViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await _vm.AutoLoad();

            _isShort = this.ActualWidth <= 640;
            WeakReferenceMessenger.Default.Send("RequestReposition", nameof(MusicInfoDisplay));
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isShort && this.ActualWidth > 640)
            {
                _isShort = false;
                WeakReferenceMessenger.Default.Send("RequestReposition", nameof(MusicInfoDisplay));
            }
            if (!_isShort && this.ActualWidth <= 640)
            {
                _isShort = true;
                WeakReferenceMessenger.Default.Send("RequestReposition", nameof(MusicInfoDisplay));
            }
        }
    }
}
