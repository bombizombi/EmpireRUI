using ReactiveUI;
using System.Reactive;

namespace EmpireRUI;

public class MainMenuViewModel : ReactiveObject, IRoutableViewModel
{
    private ReactiveCommand<Unit, Unit> _startGameCommand;
    public ReactiveCommand<Unit, Unit> StartGameCommand => _startGameCommand;
    public MainMenuViewModel( IScreen screen)
    {
        HostScreen = screen;

        _startGameCommand = ReactiveCommand.Create(() => StartGame());

    }

    private void StartGame()
    {
        //var theGame = new EmpireTheGame(); //TODO: inject this
        //game parameters are read from this main menu?
        string mapString = """
                         ooooooooooooooooooooo
                         .oo.ooo.......oo.o.o.
                         ..#................oo
                         ..ooo...............o
                         """;

        var empire = new EmpireTheGame( mapString, playerCount: 1);
        var player = empire.AddPlayer();
        var army = new Army(0, 0, player);
        player.AddUnit( army);

        //debug create standing orders
        army.DebugCreateStandingOrder(StandingOrders.LongGoto, 10, 0);
        player.AddUnit(new Army(1, 1, player));

        Army.rnd = new RandomForTesting(new double[] { 1, 1 }); //always win


        var vm = new MapViewModel(HostScreen, empire);

        HostScreen.Router.Navigate.Execute(vm);

        //as it is right now, only the view will hold a reference to the game
    }

    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "MainMenu";
}