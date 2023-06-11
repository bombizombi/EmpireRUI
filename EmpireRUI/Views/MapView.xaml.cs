using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
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

/// <summary>
/// Interaction logic for MapView.xaml
/// </summary>
public partial class MapView : MapViewBase
{
    public MapView()
    {
        InitializeComponent();

        this
            .WhenActivated(
                disposables =>
                {
                    this.tbMessages.Focus();

                    this
                        .OneWayBind(this.ViewModel, vm => vm.MapString, v => v.tbMap.Text)
                        .DisposeWith(disposables);

                    //bind interaction

                    //show async message box

                    this.ViewModel
                        ?.Confirm
                        .RegisterHandler(async interaction =>
                            {
                                await ShowAsync("Game Over", interaction.Input);

                                interaction.SetOutput(Unit.Default);
                            });

                    this.ViewModel
                        ?.interactionMove.RegisterHandler(async interaction =>
                        {
                            await GameLoopInteractionHandler(interaction);
                            //xx
                            //tempCommands = new Subject<GameOrder>();
                            //var move = await tempCommands.Take(1);
                            //interaction.SetOutput(move);
                            //tempCommands = null;
                        });

                });

        
        this
            .WhenActivated(
                disposables =>
                {
                    //_ = ViewModel?.MainGameLoop();
                    _ = ViewModel?.MainGameLoopSafe();
                });

        SetupKeyboardObservable();

    }

    private Subject<GameOrder>? tempCommands = new Subject<GameOrder>();

    private async Task GameLoopInteractionHandler(InteractionContext<string, GameOrder> interaction)
    {
        MapViewModel vm = ViewModel;

        //start the active army hearthbeat on start of every interaction
        var heartbeat = Observable.Interval(TimeSpan.FromSeconds(0.5))
            .Select(x => x % 2 == 0)
            //.Do(x => Debug.WriteLine($"heartbeat {x}"))
            //.TakeUntil(interaction.Input)
            .SubscribeOn(RxApp.MainThreadScheduler)
            .Subscribe(x =>
            {
                vm.HeartBeat(x);
                //using captured ViewModel to avoid crashing
            });

        tempCommands = new Subject<GameOrder>();
        var move = await tempCommands.Take(1);
        interaction.SetOutput(move);
        tempCommands = null;

        Debug.WriteLine($"heartbeat OFFXXXXXXXXXXXXXXxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx ");
        heartbeat.Dispose(); //turn it off
    }

    private string InteractionMoveHandler(string input)
    {
        return "InteractionMoveHandler: " + input;
    }



    public static async Task ShowAsync(string message, string caption="")
    {
        await Task.Delay(1); // Simulate an asynchronous operation
        MessageBox.Show(message, caption);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.Move, 1, 0);
        tempCommands?.OnNext(order);
        tempCommands?.OnCompleted();
        //ViewModel.TempGoRight();
    }

    private void KeyboardDirection(Point p)
    {
        //action comand order move GameOrder
        GameOrder order = new GameOrder(GameOrder.Type.Move, (int)p.X, (int)p.Y);
        tempCommands?.OnNext(order);
        tempCommands?.OnCompleted();

    }


    private void SetupKeyboardObservable()
    {
        var keyDowns = Observable.FromEventPattern<KeyEventArgs>(this, nameof(this.PreviewKeyDown))
                                  .Do(e => e.EventArgs.Handled = true);
        var keyUps = Observable.FromEventPattern<KeyEventArgs>(this, nameof(this.PreviewKeyUp));

        var leftArrowDowns =  keyDowns.Where(k => k.EventArgs.Key == Key.Left || k.EventArgs.Key == Key.A || k.EventArgs.Key == Key.NumPad4);
        var leftArrowUps =      keyUps.Where(k => k.EventArgs.Key == Key.Left || k.EventArgs.Key == Key.A || k.EventArgs.Key == Key.NumPad4);
        var rightArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.Right || k.EventArgs.Key == Key.D || k.EventArgs.Key == Key.NumPad6);
        var rightArrowUps =     keyUps.Where(k => k.EventArgs.Key == Key.Right || k.EventArgs.Key == Key.D || k.EventArgs.Key == Key.NumPad6);

        //var rotatesDowns = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad8);
        //var dropsDowns = keyDowns.Where(k => k.EventArgs.Key == Key.Space);

        var upArrowDowns =   keyDowns.Where(k => k.EventArgs.Key == Key.Up || k.EventArgs.Key == Key.W || k.EventArgs.Key == Key.NumPad8);
        var upArrowUps =       keyUps.Where(k => k.EventArgs.Key == Key.Up || k.EventArgs.Key == Key.W || k.EventArgs.Key == Key.NumPad8);
        var downArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.Down || k.EventArgs.Key == Key.X || k.EventArgs.Key == Key.NumPad2);
        var downArrowUps =     keyUps.Where(k => k.EventArgs.Key == Key.Down || k.EventArgs.Key == Key.X || k.EventArgs.Key == Key.NumPad2);

        var leftupArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad7 || k.EventArgs.Key == Key.Q);
        var leftupArrowUps   = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad7 || k.EventArgs.Key == Key.Q);
        var rightupArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad9 || k.EventArgs.Key == Key.E);
        var rightupArrowUps   = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad9 || k.EventArgs.Key == Key.E);

        var leftdownArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad1 || k.EventArgs.Key == Key.Z);
        var leftdownArrowUps   = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad1 || k.EventArgs.Key == Key.Z);
        var rightdownArrowDowns = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad3 || k.EventArgs.Key == Key.C);
        var rightdownArrowUps   = keyDowns.Where(k => k.EventArgs.Key == Key.NumPad3 || k.EventArgs.Key == Key.C);

        var leftArrowPressed = leftArrowDowns.Select(_ => -1.0)
                                             .Merge(leftArrowUps.Select(_ => 0.0))
                                             .StartWith(0.0);
        var rightArrowPressed = rightArrowDowns.Select(_ => 1.0)
                                            .Merge(rightArrowUps.Select(_ => 0.0))
                                            .StartWith(0.0);
        var upArrowPressed = upArrowDowns.Select(_ => -1.0)
                                            .Merge(upArrowUps.Select(_ => 0.0))
                                            .StartWith(0.0);
        var downArrowPressed = downArrowDowns.Select(_ => 1.0)
                                            .Merge(downArrowUps.Select(_ => 0.0))
                                            .StartWith(0.0);
        var xVelocity = leftArrowPressed.Merge(rightArrowPressed);
        var yVelocity = upArrowPressed.Merge(downArrowPressed);
        var velocity = xVelocity.CombineLatest(yVelocity, (x, y) => new Point(x, y));

        //diagonals?


        var viewModel = DataContext as MapViewModel;
        velocity
            .Where( p=> p.X != 0 || p.Y != 0)
            .Subscribe(
            p =>
            {
                Debug.WriteLine("keyboard velocity: " + p.ToString());
                this.KeyboardDirection(p);
                //viewModel. MoveArmy(new System.Drawing.Point((int)p.X, (int)p.Y));
            }
             );


        //rotatesDowns.Subscribe(_ => viewModel.RotateBlock());
        //dropsDowns.Subscribe(_ => viewModel.DropBlock());
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.Move, 1, 1);
        tempCommands?.OnNext(order);
        tempCommands?.OnCompleted();

    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.LongMove, 10, 0);
        tempCommands?.OnNext(order); tempCommands?.OnCompleted();
    }

    private void Button_Click_3(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.Sentry, -1, -1);
        tempCommands?.OnNext(order); tempCommands?.OnCompleted();
    }

    private void Button_Click_4(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.Unload, -1, -1);
        tempCommands?.OnNext(order); tempCommands?.OnCompleted();
    }

    private void Button_Click_5(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.UnsentryAll ,  -1, -1);
        tempCommands?.OnNext(order); tempCommands?.OnCompleted();
    }

    private void Button_Click_6(object sender, RoutedEventArgs e)
    {
        var order = new GameOrder(GameOrder.Type.HackChangeCityProduction, -1, -1);
        tempCommands?.OnNext(order); tempCommands?.OnCompleted();
    }
} //end class MapView



//workaround for XAML inabillity to inherit generic class
public class MapViewBase : ReactiveUserControl<MapViewModel> { }
