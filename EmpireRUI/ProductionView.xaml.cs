using DynamicData.Kernel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmpireRUI;

/// <summary>
/// Interaction logic for ProductionView.xaml
/// </summary>
public partial class ProductionView : ProductionViewBase
{
    public ProductionView()
    {
        InitializeComponent();


        //this.Bind(ViewModel, vm => vm.Option1, view => view.radioButton1.Checked);
        ////...
        //this.Bind(ViewModel, vm => vm.OptionN, view => view.radioButtonN.Checked);
        ////22



        this.WhenAnyValue(x => x.ViewModel.Production)
            .Subscribe(prod =>
            {
                RadioButton c = prod.production switch
                {
                    ProductionEnum.army => Armies,
                    ProductionEnum.figher => Fighters,
                    ProductionEnum.transport => Transporters,
                    _ => Armies
                };
                c.IsChecked = true;


            });

        this.WhenAnyValue(x => x.Armies.IsChecked, x => x.Fighters.IsChecked, x => x.Transporters.IsChecked)
            .Subscribe(values =>
            {
                var option1 = values.Item1;
                var option2 = values.Item2;
                var option3 = values.Item3;

                if ((bool)option1) ViewModel.Production.production = ProductionEnum.army;
                if ((bool)option2) ViewModel.Production.production = ProductionEnum.figher;
                if ((bool)option3) ViewModel.Production.production = ProductionEnum.transport;
            });



    }

    private void btOK_Click(object sender, RoutedEventArgs e)
    {
            //public Subject<bool> observableOK = new();

            ViewModel.OK();
    }
}

//workaround for XAML inabillity to inherit generic class
public class ProductionViewBase : ReactiveUserControl<ProductionViewModel> { }
