using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
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

        var mouseDowns = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
            h => canvas.MouseLeftButtonDown += h, h => canvas.MouseLeftButtonDown -= h);
        var mouseUps = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
            h => canvas.MouseLeftButtonUp += h, h => canvas.MouseLeftButtonUp -= h);

        var mouseMovement = mouseDowns
            .Select(e => new MouseMovement { StartPoint = e.EventArgs.GetPosition(canvas) })
            .SelectMany(startPoint =>  mouseUps
                .Select(e => new MouseMovement { StartPoint = startPoint.StartPoint, EndPoint = e.EventArgs.GetPosition(canvas) })
        );

        mouseMovement.Subscribe(mm => Debug.WriteLine($"Start: {mm.StartPoint}, End: {mm.EndPoint}"));

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
    /*    
public IObservable<MouseMovement> CreateMouseMovementObservable()
{
return mouseDowns
.Select(downEvent => new MouseMovement { StartPoint = downEvent.GetPosition(canvas) })
.SelectMany(startPoint =>
mouseUps
.Select(upEvent => new MouseMovement { StartPoint = startPoint.StartPoint, EndPoint = upEvent.GetPosition(canvas) })
);
}
*/

}
public class MouseMovement
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }
}


//workaround for XAML inabillity to inherit generic class
public class MainMenuViewBase : ReactiveUserControl<MainMenuViewModel> { }
