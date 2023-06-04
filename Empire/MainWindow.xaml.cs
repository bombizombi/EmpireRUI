using Empire.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Empire.Views;

namespace Empire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MapViewModel GetVM()
        {
            return (MapViewModel)this.DataContext;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //get the data?
            //create the model when the application startss, this might need to change as well


            var dc = this.DataContext;
            
            MapViewModel vm = (MapViewModel)dc;

            vm.DummyDel();
            //vm.DummyAdd();
            //lol
            //vm.EmpireMap.RemoveAt(0);

        }


        public static RoutedCommand DebugCmd = new RoutedCommand();
        public static RoutedCommand ProductionDialogTestCmd = new RoutedCommand();
        public static RoutedCommand AsyncTestCmd = new RoutedCommand();
        public static RoutedCommand Window2Cmd = new RoutedCommand();

        private void DebugCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DebugCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();
            vm.Debug_SmallerMap();
        }
        private void ProductionDialogTestCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ProductionDialogTestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();

            //fake show and hide production dialog


            var prodVM = new ProductionViewModel(vm);

            dialogProduction.DataContext = prodVM;
            dialogProduction.Visibility = Visibility.Visible;

            //? TODO

        }

        private void AsyncTestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();
            vm.TestAsync();



        }

        private void Window2Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var win2 = new MainWindow2(GetVM());

            win2.Show();

        }



        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            //Debugger.Break();
            FrameworkElement button = (FrameworkElement)sender;

            if (button.DataContext.GetType().FullName == "MS.Internal.NamedObject") return; //cell probably disconnected from window?

            CellViewModel cell = (CellViewModel) button.DataContext;
            GetVM().CellClicked(cell);

//            cell.Clicked();

        }

        private CellViewModel? CellVM(object sender)
        {

            return ((Image)sender).DataContext as CellViewModel;

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

            CellVM(sender)?.OnMouseDown(e);

        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CellVM(sender)?.OnMouseUp(e);
        }

        private void Button_EndTurn(object sender, RoutedEventArgs e)
        {
            //GetVM().app.Command_EndOfTurn();
            GetVM().Command_EndOfTurn();
        }

        private void Button_SimulateModal(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var pvm = new ProductionViewModel( GetVM());
            ProductionModalWindow win = new ProductionModalWindow(pvm);
            win.ShowDialog();
            var rez = pvm.UnitsArray;


        }
    }
}
