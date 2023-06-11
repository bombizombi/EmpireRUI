using Splat.ModeDetection;
using System.Reactive;
using System.Reactive.Linq;
using System.Xaml;

namespace EmpireRUI;

public class MapViewModel : ReactiveObject, IRoutableViewModel
{
    public static int classCount = 0;

    private EmpireTheGame empire;

    //direct property
    public string MapStringDirect => empire.Players[0].Dump();

    //property from observable
    private readonly ObservableAsPropertyHelper<string> mapString;
    public string MapString => this.mapString.Value;

    private MapViewModel()
    {
        classCount++;
    }

    public MapViewModel(IScreen screen, EmpireTheGame e)
    {
        classCount++;
        if (classCount > 1) { Debugger.Break(); }

        HostScreen = screen;
        empire = e;

        //mapString = empire.Players[0].DumpObs.ToProperty(this, x => x.MapString);
        //move to the right thread right before subscribing
        mapString = empire.Players[0].DumpObs
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.MapString);

        confirm = new Interaction<string, Unit>();



        /*mapString = empire.Players[0]
            .WhenAnyValue(x => x.Dump)
            .ToProperty(this, x => x.MapString);*/

    }
    public string UrlPathSegment { get; } = "Map";
    public IScreen HostScreen { get; }

    private bool loopStarted = false;
    public async Task MainGameLoopSafe()
    {
        if (loopStarted) { return; }
        loopStarted = true;
        await MainGameLoop();
        loopStarted = false;
    }



    public async Task MainGameLoop()
    {
        //this method could belong to the game model

        tempCommands.Subscribe(x => {
            Debug.WriteLine("tempCommands: " + x);
            //empire.Players[0].GoRight());
        });


        int count = 0;

        bool gameOver = false;
        do
        {
            //check new cities
            //report production?
            //activate unit?
            //start interaction
            //send interaction result(game move?) to the game model

            await CheckConqueredCities();


            //var army = empire.Players[0].ActivateUnit();
            var army = empire.ActivePlayer.ActivateUnit();
            if(army is not null && army.IsFlashing) { Debugger.Break();  }
            //ActivateUnit will also execute standing orders which means we need some delays to display moves


            if ( army is not null)
            {
                Debug.WriteLine($"Activated army {army.Name} at {army.X},{army.Y}.");


                Debug.WriteLine($"before requesting interaction {count}. ");
                var tempMove2 = await interactionMove.Handle("");
                Debug.WriteLine("tempCommands: " + tempMove2.ToString());

                empire.GameMove(tempMove2);
                await CheckConqueredCities();
            }
            else
            {
                Debug.WriteLine("No army activated.");
                empire.Players[0].NewMove();
            }


            //await Observable.Interval(TimeSpan.FromSeconds(1)).Take(2);

            gameOver = empire.Players[0].IsDead(); 
            Debug.WriteLine($"main game loop {count} - gameOver: {gameOver} move number: {empire.ActivePlayer.MoveNumber}");
            count++;
        } while( !gameOver );


        var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
        var rez = await ProductionInteraction.Handle(prod.Production);


        //-loop while there are still units with steps left

        //execute simple interaction 

        await confirm.Handle(rez.production.ToString());

        //move to gameover screen
        var vm = new GameOverViewModel(HostScreen, empire);
        HostScreen.Router.Navigate.Execute(vm);
        //HostScreen.Router.Navigate.Execute(vm);



    }

    private async Task CheckConqueredCities()
    {
        //uninitialized cities really don't care what you change or don't change in the dialog
        //cities that are already producing something should not reset production if the production type was not changed
        var cities = empire.ActivePlayer.GetCities()
            .Where(c => c.production == ProductionEnum.uninitialized);
        foreach (var c in cities)
        {
            var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
            var rez = await ProductionInteraction.Handle(prod.Production);
            c.SetProduction( (int)rez.production);

        }

        var requestsForChange = empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest);

        foreach (var c in requestsForChange)
        {
            c.ChangeRequest = false; 
            var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
            var rez = await ProductionInteraction.Handle(prod.Production);
            if( c.production != rez.production)
            {
                c.SetProduction((int)rez.production);
            }

        }

        //var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
        //var rez = await ProductionInteraction.Handle(prod.Production);

    }


    public void HeartBeat(bool visible)
    {
        empire.ActivePlayer.RenderOnHearbeat(visible);
    }


    public Interaction<string, GameOrder> interactionMove = new(); //property?
    //string imput parameter is unused

    private Subject<string>tempCommands = new Subject<string>();
    internal void TempGoRight()
    {
        tempCommands.OnNext("GoRight");

    }

    //create a private object that will hold an interaction


    private Interaction<string, Unit> confirm;
    public Interaction<string, Unit> Confirm => confirm;


    private Interaction<ProductionData, ProductionData> productionInteraction = new Interaction<ProductionData, ProductionData>();
    public Interaction<ProductionData, ProductionData> ProductionInteraction => productionInteraction;


    } // end class MapView Model
