using CommunityToolkit.Mvvm.DependencyInjection;

using HappyStudio.Subtitle.Control.Interface.Models;

using SimpleSongsPlayer.Services;

using System;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers.Templates
{
    public sealed partial class LyricPreviewItemTemplate : UserControl
    {
        private readonly ConfigurationService _configuration;

        private SubtitleLineUi _source;

        public LyricPreviewItemTemplate()
        {
            this.InitializeComponent();
            _configuration = Ioc.Default.GetRequiredService<ConfigurationService>();
            _configuration.PropertyChanged += Configuration_PropertyChanged;

            InitSource(this.DataContext as SubtitleLineUi);
            this.DataContextChanged += (s, e) => InitSource(e.NewValue as SubtitleLineUi);

            Root_TextBlock.Opacity = _configuration.LyricPreviewOpacity;
            Root_TextBlock.FontSize = _configuration.LyricPreviewFontSize;
        }

        private void InitSource(SubtitleLineUi newSource)
        {
            if (newSource == null)
                return;

            if (_source != null)
                _source.PropertyChanged -= Source_PropertyChanged;

            _source = newSource;
            Root_TextBlock.Text = _source.Content;
            _source.PropertyChanged += Source_PropertyChanged;
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_source == null)
                return;

            if (e.PropertyName == nameof(_configuration.LyricPreviewOpacity) && !_source.IsSelected)
                Root_TextBlock.Opacity = _configuration.LyricPreviewOpacity;

            if (e.PropertyName == nameof(_configuration.LyricPreviewFontSize))
            {
                if (_source.IsSelected)
                    Root_TextBlock.FontSize = _configuration.LyricPreviewFontSize + 2;
                else
                    Root_TextBlock.FontSize = _configuration.LyricPreviewFontSize;
            }
        }

        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_source.IsSelected))
            {
                if (_source.IsSelected)
                {
                    Root_TextBlock.FontSize = _configuration.LyricPreviewFontSize + 2;
                    Root_TextBlock.Opacity = 1;
                    Root_TextBlock.FontWeight = FontWeights.Bold;
                }
                else
                {
                    Root_TextBlock.FontSize = _configuration.LyricPreviewFontSize;
                    Root_TextBlock.Opacity = _configuration.LyricPreviewOpacity;
                    Root_TextBlock.FontWeight = FontWeights.Normal;
                }
            }
        }
    }
}
