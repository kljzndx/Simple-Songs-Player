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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class SplitButton : UserControl
    {
        public SplitButton()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty LeftContentProperty = DependencyProperty.Register(
            nameof(LeftContent), typeof(object), typeof(SplitButton), new PropertyMetadata(null));

        public static readonly DependencyProperty RightButtonStyleProperty = DependencyProperty.Register(
            nameof(RightButtonStyle), typeof(Style), typeof(SplitButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ExtendFlyoutProperty = DependencyProperty.Register(
            nameof(ExtendFlyout), typeof(FlyoutBase), typeof(SplitButton), new PropertyMetadata(null));

        public object LeftContent
        {
            get => (object) GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }

        public Style RightButtonStyle
        {
            get => (Style) GetValue(RightButtonStyleProperty);
            set => SetValue(RightButtonStyleProperty, value);
        }

        public FlyoutBase ExtendFlyout
        {
            get => (FlyoutBase) GetValue(ExtendFlyoutProperty);
            set => SetValue(ExtendFlyoutProperty, value);
        }

        public event RoutedEventHandler LeftButton_Click;

        private void Left_Button_OnClick(object sender, RoutedEventArgs e)
        {
            LeftButton_Click?.Invoke(this, e);
        }

        private void Right_Button_OnClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(Root_StackPanel);
        }
    }
}
