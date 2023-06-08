namespace EmpireRUI;

public class GameOverViewModel : ReactiveObject, IRoutableViewModel
{

    private EmpireTheGame empire;
    public GameOverViewModel(IScreen screen, EmpireTheGame e) 
    { 
        Host = screen; 
        empire = e; 
    }


    
    IScreen Host { get; set; }
    public string? UrlPathSegment => "Gameover";
    public IScreen HostScreen => throw new NotImplementedException();
}
