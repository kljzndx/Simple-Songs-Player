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
using SimpleSongsPlayer.Service;

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
        
        public event PointerEventHandler PositionSlider_PointerPressed;
        public event PointerEventHandler PositionSlider_PointerReleased;

        public event PointerEventHandler RewindButton_PointerPressed;
        public event PointerEventHandler RewindButton_PointerReleased;

        public event PointerEventHandler FastForwardButton_PointerPressed;
        public event PointerEventHandler FastForwardButton_PointerReleased;

        public event TypedEventHandler<CustomTransportControls, KeyValuePair<int, string>> RepeatModeSelectionChanged;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.LogByObject("监听进度条的指针事件");
            {
                var progressSlider = (Slider) GetTemplateChild("ProgressSlider");
                progressSlider.AddHandler(PointerPressedEvent, new PointerEventHandler((s, e) => PositionSlider_PointerPressed?.Invoke(s, e)), true);
                progressSlider.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => PositionSlider_PointerReleased?.Invoke(s, e)), true);
                progressSlider.AddHandler(PointerCanceledEvent, new PointerEventHandler((s, e) => PositionSlider_PointerReleased?.Invoke(s, e)), true);
                progressSlider.AddHandler(PointerCaptureLostEvent, new PointerEventHandler((s, e) => PositionSlider_PointerReleased?.Invoke(s, e)), true);

            }

            this.LogByObject("监听快退按钮的指针事件");
            {
                var rewindButton = (AppBarButton) GetTemplateChild("Rewind_Button");
                rewindButton.AddHandler(PointerPressedEvent, new PointerEventHandler((s, e) => RewindButton_PointerPressed?.Invoke(s, e)), true);
                rewindButton.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => RewindButton_PointerReleased?.Invoke(s, e)), true);
                rewindButton.AddHandler(PointerCanceledEvent, new PointerEventHandler((s, e) => RewindButton_PointerReleased?.Invoke(s, e)), true);
                rewindButton.AddHandler(PointerCaptureLostEvent, new PointerEventHandler((s, e) => RewindButton_PointerReleased?.Invoke(s, e)), true);
            }

            this.LogByObject("监听快进按钮的指针事件");
            {
                var fastForwardButton = (AppBarButton) GetTemplateChild("FastForward_Button");
                fastForwardButton.AddHandler(PointerPressedEvent, new PointerEventHandler((s, e) => FastForwardButton_PointerPressed?.Invoke(s, e)), true);
                fastForwardButton.AddHandler(PointerReleasedEvent, new PointerEventHandler((s, e) => FastForwardButton_PointerReleased?.Invoke(s, e)), true);
                fastForwardButton.AddHandler(PointerCanceledEvent, new PointerEventHandler((s, e) => FastForwardButton_PointerReleased?.Invoke(s, e)), true);
                fastForwardButton.AddHandler(PointerCaptureLostEvent, new PointerEventHandler((s, e) => FastForwardButton_PointerReleased?.Invoke(s, e)), true);
            }

            this.LogByObject("监听循环模式的选择更改事件");
            {
                var repeatModeListBox = (ListBox) GetTemplateChild("RepeatMode_ListBox");
                repeatModeListBox.SelectionChanged += (s, e) =>
                {
                    var sender = (ListBox) s;
                    RepeatModeSelectionChanged?.Invoke(this, new KeyValuePair<int, string>(sender.SelectedIndex, ((ListBoxItem)sender.SelectedItem).Content.ToString()));
                };
            }
        }
    }
}
