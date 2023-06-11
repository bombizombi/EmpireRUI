using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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


public partial class MainMenuView : MainMenuViewBase
{

    public MainMenuView()
    {
        InitializeComponent();

        this 
            .WhenAnyValue(x => x.ViewModel)
            .Where(x => x != null)
            .Subscribe(x => DataContext = x);
        this.WhenActivated(disposables =>
        {
//            this.Bind(ViewModel, vm => vm.UserName, v => v.Username.Text)
  //              .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.StartGameCommand, v => v.newGame)
            .DisposeWith (disposables);

            this.BindCommand(ViewModel, vm => vm.ConquerTheWorld, v => v.conquerTheWorld)
            .DisposeWith (disposables);
           

            // Dispose WhenAny bindings to ensure we won't have memory
            // leaking here if the ViewModel outlives the View and vice 
            // versa.
            //this.WhenAnyValue(v => v.ViewModel.IsBusy)
            //    .BindTo(this, v => v.ProgressRing.IsActive)
            //    .DisposeWith(disposables);
        });

        //22

        //totally multiplicating downs 
        var mouseDowns = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                            h => canvas.MouseLeftButtonDown += h, h => canvas.MouseLeftButtonDown -= h)
            .Select(e => e.EventArgs.GetPosition(canvas));
        var mouseUps = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                            h => canvas.MouseLeftButtonUp += h, h => canvas.MouseLeftButtonUp -= h)
            .Select(e => e.EventArgs.GetPosition(canvas));


        //this one will work fine if you drag, but drop outside of window
        var drag = mouseDowns
            .TakeUntil(mouseUps)
            .CombineLatest(mouseUps
                .Take(1), (p1, p2) => new MouseMovement { StartPoint = p1, EndPoint = p2 })
            .Repeat();
        drag.Subscribe(mm => Debug.WriteLine($"Start: {mm.StartPoint}, End: {mm.EndPoint}"));
        

        /*
        //this one will duplicate all the drags that end up outside of window, and dump them all on the last mouse up
        var mouseMovement = mouseDowns
            .SelectMany(startPoint => mouseUps
                .Take(1)
                .Select(e => new MouseMovement { StartPoint = startPoint, EndPoint = e })
        );
        mouseMovement.Subscribe(mm => Debug.WriteLine($"Start: {mm.StartPoint}, End: {mm.EndPoint}"));
        */

        /*
        var correct = mouseDowns.CombineLatest(mouseUps, (down, up) => new MouseMovement { StartPoint = down, EndPoint = up });
        correct.Subscribe(mm => Debug.WriteLine($"Start: {mm.StartPoint}, End: {mm.EndPoint}"));
        */


    }

    private IObservable<MouseEventArgs> mouseDowns;
    private IObservable<MouseEventArgs> mouseUps;

    private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {

    }

    
    private void canvas_KeyDown(object sender, KeyEventArgs e)
    {

    }

    private void MainMenuViewBase_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {

    }

    private void canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Debug.WriteLine($"canvas_PreviewMouseLeftButtonDown {e.GetPosition(canvas)}");
    }


}
public class MouseMovement
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
}


//workaround for XAML inabillity to inherit generic class
public class MainMenuViewBase : ReactiveUserControl<MainMenuViewModel> { }
