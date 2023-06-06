global using ReactiveUI;
global using Splat;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Threading.Tasks;

namespace EmpireRUI;

public class MainViewModel : ReactiveObject, IScreen
{
    public MainViewModel()
    {
        Router = new RoutingState();


        Locator.CurrentMutable.Register( () => new MainMenuView(), typeof(IViewFor<MainMenuViewModel>));
        Locator.CurrentMutable.Register(() => new MapView(), typeof(IViewFor<MapViewModel>));

        //Debug.WriteLine("MainViewModel ctor");
        Router.ThrownExceptions.Subscribe(ex =>
        {
           Debug.WriteLine(ex);
        });


        Router.NavigateAndReset.Execute(new MainMenuViewModel(this));
    }


    public RoutingState Router { get; }

    internal void DirectButtonClick()
    {
        Router.Navigate.Execute(new MainMenuViewModel(this));
    }
}
