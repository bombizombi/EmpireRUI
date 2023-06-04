using ReactiveUI;

namespace EmpireRUI;

public class MainMenuViewModel : ReactiveObject, IRoutableViewModel
{
    public MainMenuViewModel( IScreen screen)
    {
        HostScreen = screen;
    }

    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = "MainMenu";
}