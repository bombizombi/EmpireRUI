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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Empire.ViewModels;

namespace Empire.Views
{
    /// <summary>
    /// Interaction logic for ProductionModalWindow.xaml
    /// </summary>
    public partial class ProductionModalWindow : Window
    {
        private ProductionViewModel vm;
        public ProductionModalWindow( ProductionViewModel vm)
        {
            this.vm = vm;
            InitializeComponent();
            this.DataContext = vm;
            ucProduction.ButtonOK_Clicked += OK_Clicked;
        }

        public void OK_Clicked(object sender)
        {
            this.Close();
        }
    }
}
