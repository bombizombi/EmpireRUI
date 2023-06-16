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

    private readonly ObservableAsPropertyHelper<string> messageString;
    public string MessageString => this.messageString.Value;



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

        messageString = empire.Players[0].MessageObs
            //.ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.MessageString);

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


            var army = empire.ActivePlayer.ActivateUnit();
            if(army is not null && army.IsFlashing) { Debugger.Break();  }
            //if (army?.IsFlashing) { Debugger.Break(); }
            //ActivateUnit will also execute standing orders which means we need some delays to display moves


            if ( army is not null)
            {
                //Debug.WriteLine($"Activated army {army.Name} at {army.X},{army.Y}.");


                //Debug.WriteLine($"before requesting interaction {count}. ");
                var tempMove2 = await interactionMove.Handle("");
                //Debug.WriteLine("tempCommands: " + tempMove2.ToString());

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
            //Debug.WriteLine($"main game loop {count} - gameOver: {gameOver} move number: {empire.ActivePlayer.MoveNumber}");
            count++;
        } while( !gameOver );


        ProductionData? rez;
        using (var prod = new ProductionViewModel(HostScreen, new ProductionData(), this))
        {
            rez = await ProductionInteraction.Handle(prod.Production);
        }

        //-loop while there are still units with steps left

        //execute simple interaction 
        await confirm.Handle(rez.production.ToString());

        //move to gameover screen
        var vm = new GameOverViewModel(HostScreen, empire);
        HostScreen.Router.Navigate.Execute(vm);
        //HostScreen.Router.Navigate.Execute(vm);



    }

    private IDisposable dummySolution;

    private async Task CheckConqueredCities()
    {
        //uninitialized cities really don't care what you change or don't change in the dialog
        //cities that are already producing something should not reset production if the production type was not changed
        var cities = empire.ActivePlayer.GetCities()
            .Where(c => c.production == ProductionEnum.uninitialized);
        foreach (var c in cities)
        {
            ProductionData? result;
            using (var prod = new ProductionViewModel(HostScreen, new ProductionData(), this))
            { 
                Debug.WriteLine($"11f Before starting Interaction.Handle: ");
                result = await ProductionInteraction.Handle(prod.Production);
            }
            Debug.WriteLine($"11g After await production.Handle rez is {result.production} ");
            c.SetProduction( (int)result.production);

        }



        /*

        City ccontext = null;

        empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest)
            .ToObservable()

            .Subscribe(x => Debug.WriteLine($"333sub: {x}"));

        if( dummySolution is not null)
        {
            dummySolution.Dispose();
            dummySolution = null;
        }

        var seeType1 = empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest)
            .ToObservable()
            .Select(city => Observable.Defer(() =>
            {
                Debug.WriteLine($"11a production for city {city}");
                ccontext = city;
                city.ChangeRequest = false;
                Debug.WriteLine($"11b production for city {city}");
                var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
                Debug.WriteLine($"11c production for city {city}");
                return ProductionInteraction.Handle(prod.Production);
            }));
        var seeType2 = empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest)
            .ToObservable()
            .Select(city => Observable.Defer(() =>
            {
                Debug.WriteLine($"11a production for city {city}");
                ccontext = city;
                city.ChangeRequest = false;
                Debug.WriteLine($"11b production for city {city}");
                var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
                Debug.WriteLine($"11c production for city {city}");
                return ProductionInteraction.Handle(prod.Production);
            }))
            .Concat();



        var requestsForChange = empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest)
            .ToObservable()
            .Select(city => Observable.Defer(() =>
            {
                //Debug.WriteLine($"11a production for city {city}");
                ccontext = city;
                city.ChangeRequest = false;
                //Debug.WriteLine($"11b production for city {city}");
                var prod = new ProductionViewModel(HostScreen, new ProductionData(), this);
                //Debug.WriteLine($"11c production for city {city}");
                return ProductionInteraction.Handle(prod.Production);
            }))
            .Concat()
            .Do(prod =>
            {
                ccontext.SetProduction((int)prod.production);
                //Debug.WriteLine($"22 production for city {ccontext} p: {prod.production}");
            })
            .Subscribe(x => Debug.WriteLine($"sub: {x}"));

        //Task.Delay(2000).Wait();

        dummySolution = requestsForChange;

        Debug.WriteLine($"check conquered cities, at the end {ccontext} qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqq");

        */

         //this is the original code that worked just fine
        var requestsForChange = empire
            .ActivePlayer.GetCities()
            .Where(c => c.ChangeRequest);
        foreach (var c in requestsForChange)
        {
            c.ChangeRequest = false;
            ProductionData? result;
            using (var prod = new ProductionViewModel(HostScreen, new ProductionData(), this))
            {
                result = await ProductionInteraction.Handle(prod.Production);
            }
            if (c.production != result.production)
            {
                c.SetProduction((int)result.production);
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


    //private MyInteraction<ProductionData, ProductionData> productionInteraction = new MyInteraction<ProductionData, ProductionData>();
    private MyInteraction<ProductionData, ProductionData> productionInteraction = new();
    public MyInteraction<ProductionData, ProductionData> ProductionInteraction => productionInteraction;


    } // end class MapView Model
