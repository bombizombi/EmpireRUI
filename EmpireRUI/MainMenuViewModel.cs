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
                         oo.
                         ..o
                         ..#
                         """;

        var empire = new EmpireTheGame( mapString, playerCount: 1);
        var player = new Player(empire);
        empire.Players[0] = player; //TODO: improve this



        var vm = new MapViewModel(HostScreen, empire);

        HostScreen.Router.Navigate.Execute(vm);

        //as it is right now, only the view will hold a reference to the game
    }

    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "MainMenu";
}