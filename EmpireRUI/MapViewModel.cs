using Splat.ModeDetection;
using System.Reactive;
using System.Reactive.Linq;
using System.Xaml;

namespace EmpireRUI;

public class MapViewModel : ReactiveObject, IRoutableViewModel
{
    private EmpireTheGame empire;

    //direct property
    public string MapStringDirect => empire.Players[0].Dump();

    //property from observable
    private readonly ObservableAsPropertyHelper<string> mapString;
    public string MapString => this.mapString.Value;



    public MapViewModel(IScreen screen, EmpireTheGame e)
    {
        HostScreen = screen;
        empire = e;

        mapString = empire.Players[0].DumpObs.ToProperty(this, x => x.MapString);

        confirm = new Interaction<string, Unit>();



        /*mapString = empire.Players[0]
            .WhenAnyValue(x => x.Dump)
            .ToProperty(this, x => x.MapString);*/

    }
    public string UrlPathSegment { get; } = "Map";
    public IScreen HostScreen { get; }

    public async Task StartMainGameLoop()
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

            var army = empire.Players[0].ActivateUnit();
            if( army is not null)
            {
                Debug.WriteLine($"Activated army {army.Name}.");


                Debug.WriteLine($"before requesting interaction {count}. ");
                var tempMove2 = await interactionMove.Handle("");
                Debug.WriteLine("tempCommands: " + tempMove2.ToString());

                empire.GameMove(tempMove2);

            }
            else
            {
                Debug.WriteLine("No army activated.");
                empire.Players[0].NewMove();
            }


            //await Observable.Interval(TimeSpan.FromSeconds(1)).Take(2);

            gameOver = empire.Players[0].IsDead(); 
            Debug.WriteLine("main game loop {count} - gameOver: " + gameOver);
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
