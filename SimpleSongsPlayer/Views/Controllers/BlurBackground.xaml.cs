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
using Microsoft.Toolkit.Uwp.UI.Animations.Behaviors;
using Blur = Microsoft.Toolkit.Uwp.UI.Animations.Effects.Blur;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class BlurBackground : UserControl
    {
        public BlurBackground()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(ImageSource), typeof(BlurBackground), new PropertyMetadata(null));

        public ImageSource Source
        {
            get => (ImageSource) GetValue(SourceProperty);
            set
            {
                SetValue(SourceProperty, value);

                FadeOut_Storyboard.Begin();
            }
        }

        public static readonly DependencyProperty BlurDegreeProperty = DependencyProperty.Register(
            nameof(BlurDegree), typeof(double), typeof(BlurBackground), new PropertyMetadata(10D));

        public double BlurDegree
        {
            get => (double) GetValue(BlurDegreeProperty);
            set => SetValue(BlurDegreeProperty, value);
        }

        public static readonly DependencyProperty ImageTransparencyProperty = DependencyProperty.Register(
            nameof(ImageTransparency), typeof(double), typeof(BlurBackground), new PropertyMetadata(1D));

        public double ImageTransparency
        {
            get => (double) GetValue(ImageTransparencyProperty);
            set => SetValue(ImageTransparencyProperty, value);
        }

        private void FadeOut_Storyboard_OnCompleted(object sender, object e)
        {
            Main_Image.Source = Source;
            FadeIn_Storyboard.Begin();
        }
    }
}
