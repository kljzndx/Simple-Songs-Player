using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SimpleSongsPlayer.Models
{
    public class SymbolIndexItemStyleSelector : StyleSelector
    {
        private Style emptyStyle;
        private Style haveStyle;

        public SymbolIndexItemStyleSelector()
        {
            haveStyle = new Style {TargetType = typeof(GridViewItem)};
            emptyStyle = new Style {TargetType = typeof(GridViewItem)};
            Setter widthSetter = new Setter {Property = FrameworkElement.MinWidthProperty, Value = 60D};
            Setter heightSetter = new Setter {Property = FrameworkElement.MinHeightProperty, Value = 50D};
            Setter opacitySetter = new Setter
            {
                Property = UIElement.OpacityProperty,
                Value = 0.3D
            };

            emptyStyle.Setters.Add(widthSetter);
            emptyStyle.Setters.Add(heightSetter);
            emptyStyle.Setters.Add(opacitySetter);

            haveStyle.Setters.Add(widthSetter);
            haveStyle.Setters.Add(heightSetter);
        }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            ICollectionViewGroup gp = item as ICollectionViewGroup;
            SongsGroup sg = gp.Group as SongsGroup;

            return sg.IsAny ? haveStyle : emptyStyle;
        }
    }
}