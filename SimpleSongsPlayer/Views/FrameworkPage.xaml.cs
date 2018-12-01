using System;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models.Attributes;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        private static readonly Type ClassifyPageType = typeof(MusicClassifyPage);

        private readonly SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();

        public FrameworkPage()
        {
            this.InitializeComponent();
            Main_Frame.Navigate(ClassifyPageType);
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;
        }

        private void Main_Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            var titleAttribute = e.SourcePageType.GetTypeInfo().GetCustomAttribute<PageTitleAttribute>();
            if (titleAttribute != null)
            {
                TitleBar_Grid.Visibility = Visibility.Visible;
                Title_TextBlock.Text = titleAttribute.GetTitle();
                Back_Button.Visibility = Main_Frame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                TitleBar_Grid.Visibility = Visibility.Collapsed;
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Main_Frame.CanGoBack)
            {
                if (e != null)
                    e.Handled = true;

                Main_Frame.GoBack();
            }
        }

        private void Back_Button_OnClick(object sender, RoutedEventArgs e)
        {
            Main_Frame.GoBack();
        }
    }
}
