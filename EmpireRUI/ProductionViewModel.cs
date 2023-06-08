using System.Reactive.Linq;

namespace EmpireRUI;

public class ProductionViewModel : ReactiveObject, IRoutableViewModel
{
    private ProductionData productionData;
    public ProductionViewModel(IScreen screen, ProductionData pd, MapViewModel mvm)
    {
        HostScreen = screen;
        //empire = e;
        productionData = pd;

        mvm.ProductionInteraction.RegisterHandler(async interaction =>
        {
            observableOK.Subscribe(x => Debug.WriteLine($"observableOK: {x}"));

            HostScreen.Router.Navigate.Execute(this);

            bool ok = await observableOK;

            if (ok) 
            { 
                interaction.SetOutput(productionData);
            }
        });


    }
    public Subject<bool> observableOK = new();

    public ProductionData Production
    {
        get => productionData;
        set => this.RaiseAndSetIfChanged(ref productionData, value);
    }

    public string? UrlPathSegment => "Production";
    public IScreen HostScreen { get; set; }

    internal void OK()
    {
        observableOK.OnNext(true);
        observableOK.OnCompleted();
    }
    internal void Cancel()
    {
        observableOK.OnNext(false);
        observableOK.OnCompleted();
    }

}


public class ProductionData
{

    public ProductionEnum production;

}


public enum ProductionEnum
{
    army = 0,
    figher,
    destroyer,
    transport,
    submarine,
    cruiser,
    carrier,
    battleship,
    uninitialized
}

