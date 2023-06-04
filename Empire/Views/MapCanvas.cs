using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Empire.ViewModels;

namespace Empire.Views
{
    public class MapCanvas : Canvas
    {

        internal static int BlockSize => 30;

        internal int ColumnCount => (int)Math.Floor(ActualWidth / BlockSize);

        internal int RowCount => (int)Math.Floor(ActualHeight / BlockSize);

        internal double CalculateLeft(FrameworkElement blockContainer)
        { //??
            if (blockContainer == null)
                throw new ArgumentNullException("blockContainer");

            var block = blockContainer.DataContext as CellViewModel;
            if (block == null)
                throw new ArgumentException("Element does not have a MapViewModel as its DataContext.", "blockContainer");

            return BlockSize * block.x;
        }

        internal double CalculateTop(FrameworkElement bubbleContainer)
        {
            if (bubbleContainer == null)
                throw new ArgumentNullException("bubbleContainer");

            var block = bubbleContainer.DataContext as CellViewModel;
            if (block == null)
                throw new ArgumentException("Element does not have a MapViewModel as its DataContext.", "blockContainer");

            return BlockSize * block.y;
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            var contentPresenter = visualAdded as ContentPresenter;
            if (contentPresenter != null)
            {
                var block = contentPresenter.DataContext as CellViewModel;
                if (block != null)
                {
                    SetLeft(contentPresenter,  block.x * BlockSize);
                    SetTop(contentPresenter, block.y * BlockSize);

                    contentPresenter.Width = BlockSize;
                    contentPresenter.Height = BlockSize;
                }
            }

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }


    }

}
