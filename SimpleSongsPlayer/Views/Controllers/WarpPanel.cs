using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SimpleSongsPlayer.Views.Controllers
{
    // Source: https://www.cnblogs.com/arcsinw/p/5272913.html
    public class WarpPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            //可用空间大小
            Size usefulSize = new Size(availableSize.Width, double.PositiveInfinity);

            //控件高度
            double y = 0;
            double x = 0;

            foreach (UIElement item in Children)
            {
                item.Measure(usefulSize);

                Size itemSize = item.DesiredSize;
                double itemWidth = itemSize.Width;

                y = (itemSize.Height) > y ? itemSize.Height : y;

                //加入该子控件后一行满了
                if (x + itemSize.Width > availableSize.Width)
                {
                    x = 0;
                    y += itemSize.Height;
                }
                x += itemSize.Width;
            }

            return new Size(availableSize.Width, y);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //记录横坐标
            double x = 0.0;
            double y = 0.0;

            foreach (UIElement item in Children)
            {
                Size itemSize = item.DesiredSize;
                double itemWidth = itemSize.Width;

                //加入该控件后一行满了
                if (x + itemSize.Width > finalSize.Width)
                {
                    x = 0;
                    y += itemSize.Height;
                }
                //控件的坐标
                Point pt = new Point(x, y);

                //控件布局
                item.Arrange(new Rect(pt, itemSize));
                x += itemSize.Width;
            }

            return finalSize;
        }
    }
}