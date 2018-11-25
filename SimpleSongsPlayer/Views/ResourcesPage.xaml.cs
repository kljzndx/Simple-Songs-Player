using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ResourcesPage : Page
    {
        public static ResourcesPage Current;

        public MusicLibraryService<MusicFile, MusicFileFactory> MusicFilesService;
        public UserFavoriteService UserFavoriteService;

        public ObservableCollection<MusicFileDTO> MusicFilesList = new ObservableCollection<MusicFileDTO>();
        public ObservableCollection<MusicFileGroup> UserFavoritesList = new ObservableCollection<MusicFileGroup>();

        public ResourcesPage()
        {
            this.InitializeComponent();
            Current = this;
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            MusicFilesService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            UserFavoriteService = UserFavoriteService.GetService(MusicFilesService);

            MusicFilesService.GetFiles().ForEach(mf => MusicFilesList.Add(new MusicFileDTO(mf)));

            foreach (var grouping in UserFavoriteService.GetFiles())
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in grouping)
                    files.Add(new MusicFileDTO(file));

                UserFavoritesList.Add(new MusicFileGroup(grouping.Key, files, await files.First().GetAlbumCover()));
            }

            MusicFilesService.FilesAdded += MusicFilesService_FilesAdded;
            MusicFilesService.FilesRemoved += MusicFilesService_FilesRemoved;
            UserFavoriteService.FilesAdded += UserFavoriteService_FilesAdded;
            UserFavoriteService.FilesRemoved += UserFavoriteService_FilesRemoved;
        }
        
        private void MusicFilesService_FilesAdded(object sender, IEnumerable<MusicFile> e)
        {
            foreach (var musicFile in e)
                MusicFilesList.Add(new MusicFileDTO(musicFile));
        }

        private void MusicFilesService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            foreach (var musicFile in e)
                MusicFilesList.Remove(new MusicFileDTO(musicFile));
        }

        private async void UserFavoriteService_FilesAdded(object sender, IEnumerable<IGrouping<string, MusicFile>> e)
        {
            foreach (var group in e)
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in group)
                    files.Add(new MusicFileDTO(file));

                var fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup != null)
                    files.ForEach(fileGroup.Items.Add);
                else
                    UserFavoritesList.Add(new MusicFileGroup(group.Key, files, await files.First().GetAlbumCover()));
            }
        }

        private void UserFavoriteService_FilesRemoved(object sender, IEnumerable<IGrouping<string, MusicFile>> e)
        {
            foreach (var group in e)
            {
                MusicFileGroup fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup is null)
                    continue;

                if (fileGroup.Items.Count >= group.Count())
                    UserFavoritesList.Remove(fileGroup);
                else
                {
                    List<MusicFileDTO> files = new List<MusicFileDTO>();
                    foreach (var file in group)
                        files.Add(new MusicFileDTO(file));

                    files.ForEach(mf => fileGroup.Items.Remove(mf));
                }

            }
        }
    }
}
