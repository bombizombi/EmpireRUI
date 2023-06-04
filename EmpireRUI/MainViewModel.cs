using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireRUI;

public class MainViewModel : ReactiveObject, IScreen
{
    public MainViewModel()
    {
        Router = new RoutingState();

        Locator.CurrentMutable.Register( () => new MainMenuView(), typeof(IViewFor<MainMenuViewModel>));
        Debug.WriteLine("MainViewModel ctor");
        Router.ThrownExceptions.Subscribe(ex =>
        {
           Debug.WriteLine(ex);
        });


        Router.Navigate.Execute(new MainMenuViewModel(this));
    }


    public RoutingState Router { get; }

    internal void DirectButtonClick()
    {
        Router.Navigate.Execute(new MainMenuViewModel(this));
    }
}
