using CommunityToolkit.Mvvm.ComponentModel;

using HappyStudio.Parsing.Subtitle;
using HappyStudio.Parsing.Subtitle.Interfaces;
using HappyStudio.Subtitle.Control.Interface;
using HappyStudio.Subtitle.Control.Interface.Models;
using HappyStudio.UwpToolsLibrary.Auxiliarys.Files;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicInfoViewModel : ObservableRecipient
    {
        private readonly MusicFileManageService _manageService;

        private MusicUi _musicSource;
        private BitmapSource _coverSource;
        private IEnumerable<ISubtitleLineUi> _subtitleSource;

        public MusicInfoViewModel(MusicFileManageService manageService)
        {
            _manageService = manageService;

            Messenger.Register<MusicInfoViewModel, string, string>(this, nameof(PlaybackListManageService), async (vm, mes) =>
            {
                if (mes == "CurrentPlayChanged")
                    await vm.AutoLoad();
            });
        }

        public MusicUi MusicSource
        {
            get => _musicSource;
            private set => SetProperty(ref _musicSource, value);
        }

        public BitmapSource CoverSource
        {
            get => _coverSource;
            private set => SetProperty(ref _coverSource, value);
        }

        public IEnumerable<ISubtitleLineUi> SubtitleListSource
        {
            get => _subtitleSource;
            private set => SetProperty(ref _subtitleSource, value);
        }

        public async Task AutoLoad()
        {
            MusicSource = _manageService.GetCurrentPlayItem();
            CoverSource = null;
            SubtitleListSource = new List<SubtitleLineUi>();

            if (MusicSource == null)
                return;

            CoverSource = await MusicSource.GetCoverAsync();

            var filePathArray = new string[]
            {
                Path.ChangeExtension(MusicSource.FilePath, ".lrc"),
                Path.ChangeExtension(MusicSource.FilePath, ".srt")
            };

            StorageFile file = null;
            foreach (var filePath in filePathArray)
            {
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(filePath);
                    if (file != null)
                        break;
                }
                catch (Exception)
                {
                    continue;
                }
            }

            if (file == null)
                return;

            string text = await FileReader.ReadText(file, "GBK");
            try
            {
                var stl = SubtitleParser.Parse(text);
                SubtitleListSource = stl.Lines.Select(l => new SubtitleLineUi(l)).ToList();
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
