using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel.Resources;

namespace SimpleSongsPlayer.Services
{
    public class StringResourceService
    {
        public ResourceLoader Backside { get; } = ResourceLoader.GetForCurrentView("Backside");
    }
}
