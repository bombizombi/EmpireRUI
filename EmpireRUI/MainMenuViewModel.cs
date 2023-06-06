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

        var vm = new MapViewModel(HostScreen);

        HostScreen.Router.Navigate.Execute(vm);
    }

    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "MainMenu";
}