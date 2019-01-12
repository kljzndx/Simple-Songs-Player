using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels.Selectors
{
    public class MusicGroupDynamicStyleSelector : GroupStyleSelector
    {
        public GroupStyle Normal { get; set; }
        public GroupStyle NoName { get; set; }

        protected override GroupStyle SelectGroupStyleCore(object group, uint level)
        {
            var mg = group as ICollectionViewGroup;
            if (mg is null || String.IsNullOrWhiteSpace(((MusicFileGroupDynamic) mg.Group).Name))
                return NoName;

            return Normal;
        }
    }
}