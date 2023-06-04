using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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

namespace EmpireRUI;

public class MainViewBase : ReactiveWindow<MainViewModel> { } //workaround for XAML inabillity to inherit generic class 


public partial class MainView : MainViewBase
{
    public MainView()
    {
        InitializeComponent();


        //var x = new RoutedViewHost()
        //{
        //    Router = null;// new AppBootstrapper().Router;
        //}
        //x .Router.Navigate.Execute(new MapViewModel());)



        this
            .WhenActivated(disposables =>
            {
                //here DataContext is set, but ViewModel is null
                ViewModel = (MainViewModel)DataContext;

                this.OneWayBind(ViewModel, 
                        vm => vm.Router, 
                        v => v.MyRoutedViewHost.Router)
                    .DisposeWith(disposables);
            });

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        (DataContext as MainViewModel)?.DirectButtonClick();
     //   ViewModel.DirectButtonClick();
    }
}


