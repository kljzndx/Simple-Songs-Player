using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class InputDialog : ContentDialog
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(InputDialog), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty NoInputsProperty = DependencyProperty.Register(
            nameof(NoInputs), typeof(ObservableCollection<string>), typeof(InputDialog), new PropertyMetadata(new ObservableCollection<string> { "<", ">", "|", "\\", "/", "?", ";", ":", "'", "\"", "*" }));

        private string text = String.Empty;

        public InputDialog()
        {
            this.InitializeComponent();

            ResourceLoader loader = ResourceLoader.GetForCurrentView("InputDialog");
            if (String.IsNullOrWhiteSpace(this.PrimaryButtonText))
                this.PrimaryButtonText = loader.GetString("OK");

            if (String.IsNullOrWhiteSpace(this.SecondaryButtonText))
                this.SecondaryButtonText = loader.GetString("Cancel");

            InitStatus();
        }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                Main_TextBox.Text = value;
            }
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public ObservableCollection<string> NoInputs
        {
            get => (ObservableCollection<string>)GetValue(NoInputsProperty);
        }

        private void InitStatus()
        {
            this.IsPrimaryButtonEnabled = !String.IsNullOrWhiteSpace(Main_TextBox.Text);
            ErrorInfo_TextBlock.Visibility = Visibility.Collapsed;
        }

        private void Main_TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var str in NoInputs)
            {
                if (Main_TextBox.Text.Contains(str))
                {
                    this.IsPrimaryButtonEnabled = false;
                    ErrorInfo_TextBlock.Visibility = Visibility.Visible;
                    return;
                }
            }

            InitStatus();
        }

        private void Main_TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.Text = Main_TextBox.Text.Trim();
        }

        private void InputDialog_OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            Main_TextBox.Text = String.Empty;
        }
    }
}
