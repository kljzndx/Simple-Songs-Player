﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using SimpleSongsPlayer.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.ItemTemplates
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ScrollLyricsPreviewItemTemplate : UserControl
    {
        public static readonly DependencyProperty LyricTextProperty = DependencyProperty.Register(
            nameof(LyricText), typeof(string), typeof(ScrollLyricsPreviewItemTemplate), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(ScrollLyricsPreviewItemTemplate), new PropertyMetadata(false));

        private readonly SettingProperties settings = SettingProperties.Current;

        public ScrollLyricsPreviewItemTemplate()
        {
            this.InitializeComponent();
            settings.PropertyChanged += Settings_PropertyChanged;
        }

        public bool IsSelected
        {
            get => (bool) GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);

                Root_TextBlock.FontWeight = value ? FontWeights.Bold : FontWeights.Normal;
                Root_TextBlock.FontSize = IsSelected ? settings.ScrollLyrics_FontSize + 2 : settings.ScrollLyrics_FontSize;
            }
        }

        public string LyricText
        {
            get => (string) GetValue(LyricTextProperty);
            set => SetValue(LyricTextProperty, value);
        }

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
