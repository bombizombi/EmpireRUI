using Empire.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Empire.Views
{
    /// <summary>
    /// Interaction logic for MapBlock.xaml
    /// </summary>
    public partial class MapBlock : UserControl
    {


        public bool dead = false;
        public bool MarkAsDead() { dead = true; return true; }
        public bool MarkAsAlive() { dead = false; return true; }

        public MapBlock()
        {
            InitializeComponent();
        }


        //blinking
        public void StartAnimationForActiveCell()
        {
            Storyboard? s = this.Resources["animBlinking"] as Storyboard;
            if( s is not null) s.Begin();
        }
        public void StopAnimationForActiveCell()
        {
            Storyboard? s = this.Resources["animBlinking"] as Storyboard;
            if( s is not null) s.Stop();

        }

        //delay
        public async Task StartAnimationDelay()
        {
            //Storyboard? s = this.Resources["animDelayAfterMove"] as Storyboard;
            Storyboard? s = this.Resources["animDelayInbetweenMove"] as Storyboard;
            await s.BeginAsync();
        }

        public Storyboard? GetStoryboard(string sb)
        {
            Storyboard? s  = this.Resources[sb] as Storyboard;
            return s; 
        }


        private CellViewModel CellVM(object sender)
        {
            //return ((Image)sender).DataContext as CellViewModel;
            FrameworkElement fe = (FrameworkElement)sender;
            return fe.DataContext as CellViewModel;
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CellVM(sender)?.OnMouseDown(e);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //view -> viewModel call
            CellVM(sender)?.OnMouseUp(e);
        }
    }
    }
