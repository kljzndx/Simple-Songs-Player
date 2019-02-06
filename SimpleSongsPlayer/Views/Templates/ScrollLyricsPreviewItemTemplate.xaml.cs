using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.ViewModels.SettingProperties;
using System.ComponentModel;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Templates
{
    public sealed partial class ScrollLyricsPreviewItemTemplate : UserControl
    {
        public static readonly DependencyProperty LyricTextProperty = DependencyProperty.Register(
            nameof(LyricText), typeof(string), typeof(ScrollLyricsPreviewItemTemplate), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(ScrollLyricsPreviewItemTemplate), new PropertyMetadata(false));

        private readonly ViewSettingProperties settings = ViewSettingProperties.Current;

        public ScrollLyricsPreviewItemTemplate()
        {
            this.InitializeComponent();
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);

                Root_TextBlock.FontWeight = value ? FontWeights.Bold : FontWeights.Normal;
                Root_TextBlock.FontSize = IsSelected ? settings.ScrollLyrics_FontSize + 2 : settings.ScrollLyrics_FontSize;

                if (value)
                    Selected?.Invoke(this, null);
            }
        }

        public string LyricText
        {
            get => (string)GetValue(LyricTextProperty);
            set => SetValue(LyricTextProperty, value);
        }

        public event TypedEventHandler<ScrollLyricsPreviewItemTemplate, object> Selected;

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(settings.ScrollLyrics_FontSize):
                    Root_TextBlock.FontSize = settings.ScrollLyrics_FontSize;
                    break;
            }
        }
    }
}
