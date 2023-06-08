using Splat.ModeDetection;
using System.Reactive;
using System.Reactive.Linq;

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
        bool gameOver = false;
        do
        {
            //check new cities
            //report production?

            //activate unit?
            //start interaction
            //send interaction result(game move?) to the game model

            gameOver = empire.Players[0].IsDead(); 

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

    //create a private object that will hold an interaction


    private Interaction<string, Unit> confirm;
    public Interaction<string, Unit> Confirm => confirm;


    private Interaction<ProductionData, ProductionData> productionInteraction = new Interaction<ProductionData, ProductionData>();
    public Interaction<ProductionData, ProductionData> ProductionInteraction => productionInteraction;


    } // end class MapView Model
