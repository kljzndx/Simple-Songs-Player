using System;
using Windows.UI.Xaml.Controls;

namespace SimpleSongsPlayer.ViewModels.Extensions
{
    public static class FrameExtension
    {
        public static void NavigateEx(this Frame frame, Type pageType, object parameter)
        {
            if (frame.SourcePageType is null || frame.SourcePageType.FullName != pageType.FullName)
                frame.Navigate(pageType, parameter);
        }
    }
}