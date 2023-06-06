using ReactiveUI;
using System.Reactive;

namespace EmpireRUI;

public class MainMenuViewModel : ReactiveObject, IRoutableViewModel
{
    private ReactiveCommand<Unit, Unit> _startGameCommand;
    public MainMenuViewModel( IScreen screen)
    {
        HostScreen = screen;

        _startGameCommand = ReactiveCommand.Create(() => HostScreen.Router.Navigate.Execute(new MapViewModel(HostScreen)));

    }

    private void StartGame()
    {
        var theGame = new EmpireTheGame(); //TODO: inject this
        //game parameters are read from this main menu?

        HostScreen.Router.Navigate.Execute(new MapViewModel(HostScreen));
    }

    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "MainMenu";
}