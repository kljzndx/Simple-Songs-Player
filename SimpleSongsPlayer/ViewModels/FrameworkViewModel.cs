using System.Collections.Generic;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels
{
    public class FrameworkViewModel : ViewModelBase
    {
        public List<LyricBlock> AllLyricBlocks { get; set; }
    }
}