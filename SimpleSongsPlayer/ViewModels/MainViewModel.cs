using CommunityToolkit.Mvvm.ComponentModel;

using HappyStudio.UwpToolsLibrary.Information;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        public MainViewModel()
        {
            AppVersion = AppInfo.Version;
        }

        public string AppVersion { get; set; }
    }
}
