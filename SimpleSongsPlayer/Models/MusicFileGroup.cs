using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Views;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileGroup : ObservableObject
    {
        private IGroupServiceBasicOptions<string, MusicFile> service;
        private string name;

        private readonly string _defaultCoverUri;
        private WeakReference<BitmapSource> coverReference = new WeakReference<BitmapSource>(null);
        private Func<MusicFileDTO, Task<BitmapSource>> _coverGetter;

        public MusicFileGroup(string name)
        {
            this.name = name;
            Items = new ObservableCollection<MusicFileDTO>();
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items)
        {
            this.name = name;
            Items = new ObservableCollection<MusicFileDTO>(items);
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items, string defaultCoverUri) : this(name, items)
        {
            _defaultCoverUri = defaultCoverUri;
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items, Func<MusicFileDTO, Task<BitmapSource>> coverGetter) : this(name, items)
        {
            _coverGetter = coverGetter;
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items, string defaultCoverUri, Func<MusicFileDTO, Task<BitmapSource>> coverGetter) : this(name, items, defaultCoverUri)
        {
            _coverGetter = coverGetter;
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

        public ObservableCollection<MusicFileDTO> Items { get; }

        public event TypedEventHandler<MusicFileGroup, KeyValuePair<string, string>> Renamed;

        public void Join(MusicFileGroup collection)
        {
            foreach (var item in collection.Items)
                if (Items.All(f => f.FilePath != item.FilePath))
                    Items.Add(item);
        }

        public async Task<BitmapSource> GetCover()
        {
            BitmapSource bitmap = null;

            if (coverReference.TryGetTarget(out bitmap))
                return bitmap;

            if (_coverGetter != null)
                bitmap = await _coverGetter.Invoke(Items.First());

            if (bitmap is null || bitmap.PixelWidth <= 1 && bitmap.PixelHeight <= 1)
            {
                if (String.IsNullOrWhiteSpace(_defaultCoverUri))
                    return null;

                bitmap = new BitmapImage(new Uri(_defaultCoverUri));
            }

            coverReference.SetTarget(bitmap);
            return bitmap;
        }

        public void SetUpService(IGroupServiceBasicOptions<string, MusicFile> service)
        {
            this.service = service;
            Renamed += MusicFileGroup_Renamed;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private async void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    await service.AddRange(name, e.NewItems.Cast<MusicFileDTO>().Select(f => f.FilePath));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (Items.Any())
                        await service.RemoveRange(name, e.NewItems.Cast<MusicFileDTO>().Select(f => f.FilePath));
                    else
                        await service.RemoveGroup(name);

                    break;
            }
        }

        private async void MusicFileGroup_Renamed(MusicFileGroup sender, KeyValuePair<string, string> args)
        {
            this.LogByObject("检测到 重命名音乐文件组 操作，正在同步至数据库");
            await service.RenameGroup(args.Key, args.Value);
        }
    }
}