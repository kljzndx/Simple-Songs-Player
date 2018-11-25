using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Views;

namespace SimpleSongsPlayer.ViewModels
{
    public class DataServer
    {
        public static DataServer Current;

        public MusicLibraryService<MusicFile, MusicFileFactory> MusicFilesService;
        public UserFavoriteService UserFavoriteService;

        public ObservableCollection<MusicFileDTO> MusicFilesList = new ObservableCollection<MusicFileDTO>();
        public ObservableCollection<MusicFileGroup> UserFavoritesList = new ObservableCollection<MusicFileGroup>();

        private DataServer()
        {
        }

        private async Task Initialize()
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
                MusicFilesList.Remove(MusicFilesList.First(mf => mf.FilePath == musicFile.Path));
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

        public static async Task<DataServer> GetServer()
        {
            if (Current is null)
            {
                Current = new DataServer();
                await Current.Initialize();
            }

            return Current;
        }
    }
}