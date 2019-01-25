using System;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NLog;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Log.Models;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.ViewModels.SettingProperties;
using SimpleSongsPlayer.Views;
using SimpleSongsPlayer.Views.Controllers;
using SimpleSongsPlayer.Views.SidePages;

namespace SimpleSongsPlayer
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        public static readonly MediaPlayer MediaPlayer;

        private static readonly Logger Logger;

        private bool _canRefreshData = true;

        static App()
        {
            Logger = LoggerService.GetLogger(LoggerMembers.App);
            Logger.Info("初始化播放模块");
            MediaPlayer = new MediaPlayer();
        }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
            this.Resuming += App_Resuming;

            LogExtension.SetUpAssembly(typeof(App).GetTypeInfo().Assembly, LoggerMembers.Ui);
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await e.Exception.ShowErrorDialog(Logger);
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            if (args.TaskInstance.Task.Name == SettingsPage.timedExitTaskName)
            {
                if (MediaPlayer.PlaybackSession != null &&
                    MediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Paused)
                    MediaPlayer.Pause();

                Exit();
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                Logger.Info("未检测到页面框架，开始初始化页面框架");
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }
                Logger.Info("为窗口应用页面框架");
                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
                Window.Current.Activated += Window_Activated;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    Logger.Info("跳转至主页面");
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(FrameworkPage), e.Arguments);
                }
                Logger.Info("激活窗口");
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
            _canRefreshData = true;
        }

        private async void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            var window = sender as Window;
            if (window == null)
                return;

            if (_canRefreshData && e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                _canRefreshData = false;

                await window.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("LoadData"));
                    await MusicFileDataServer.Current.InitializeMusicService();
                    await LyricFileDataServer.Current.Init();

                    FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("ScanMusicLibrary"));
                    await MusicLibraryFileServiceManager.Current.ScanFiles();
                    await LyricIndexDataServer.Current.ScanAsync();
                    if (!FavoritesDataServer.Current.IsInit)
                    {
                        FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("GetFavorites"));
                        await FavoritesDataServer.Current.InitializeFavoritesService();
                    }
                    if (!OtherSettingProperties.Current.IsMigratedOldFavorites)
                    {
                        FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("MigrationOldFavorites"));
                        await FavoritesDataServer.Current.MigrateOldFavorites();
                        OtherSettingProperties.Current.IsMigratedOldFavorites = true;
                    }
                    FlyoutNotification.Hide();
                });
                
                _canRefreshData = true;
            }
        }
    }
}
