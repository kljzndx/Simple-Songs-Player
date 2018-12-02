using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed class CustomTransportControls : MediaTransportControls
    {
        public static readonly DependencyProperty RepeatMode_SelectedIDProperty = DependencyProperty.Register(
            nameof(RepeatMode_SelectedID), typeof(int), typeof(CustomTransportControls), new PropertyMetadata(-1));

        public CustomTransportControls()
        {
            this.DefaultStyleKey = typeof(CustomTransportControls);
        }

        public int RepeatMode_SelectedID
        {
            get => (int) GetValue(RepeatMode_SelectedIDProperty);
            set => SetValue(RepeatMode_SelectedIDProperty, value);
        }

        public event EventHandler<object> RewindButtonClick;
        public event EventHandler<object> FastForwardButtonClick;
        public event TypedEventHandler<CustomTransportControls, KeyValuePair<int, string>> RepeatModeSelectionChanged;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            {
                var repeatModeListBox = (ListBox) GetTemplateChild("RepeatMode_ListBox");
                repeatModeListBox.SelectionChanged += (s, e) =>
                {
                    var sender = (ListBox) s;
                    RepeatModeSelectionChanged?.Invoke(this, new KeyValuePair<int, string>(sender.SelectedIndex, ((ListBoxItem)sender.SelectedItem).Content.ToString()));
                };
            }

            {
                var rewindButton = (AppBarButton) GetTemplateChild("Rewind_Button");
                rewindButton.Click += (s, e) => RewindButtonClick?.Invoke(this, null);
            }

            {
                var fastForwardButton = (AppBarButton) GetTemplateChild("FastForward_Button");
                fastForwardButton.Click += (s, e) => FastForwardButtonClick?.Invoke(this, null);
            }
        }
    }
}
