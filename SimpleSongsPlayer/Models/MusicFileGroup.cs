using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Views;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileGroup : ObservableObject
    {
        private UserFavoriteService service;
        private string name;

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items)
        {
            this.name = name;
            Items = new ObservableCollection<MusicFileDTO>(items);
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items, BitmapSource cover) : this(name, items)
        {
            Cover = cover;
        }

        public string Name
        {
            get => name;
            set
            {
                string str = name;
                Set(ref name, value);
                Renamed?.Invoke(this, new KeyValuePair<string, string>(str, value));
            }
        }

        public BitmapSource Cover { get; }
        public ObservableCollection<MusicFileDTO> Items { get; }

        public event TypedEventHandler<MusicFileGroup, KeyValuePair<string, string>> Renamed;

        public void SetUpService(UserFavoriteService service)
        {
            this.service = service;
            this.Renamed += MusicFileGroup_Renamed;
        }

        private void MusicFileGroup_Renamed(MusicFileGroup sender, KeyValuePair<string, string> args)
        {
            this.LogByObject("检测到 重命名音乐文件组 操作，正在同步至数据库");
            service.RenameGroup(args.Key, args.Value);
        }
    }
}