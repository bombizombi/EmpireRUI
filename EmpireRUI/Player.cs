using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.Pkcs;

namespace EmpireRUI;

public class Player
{
    EmpireTheGame app;
    private List<IUnit> units;
    //private List<City> cities;

    public FoggyMapElem[,] map;

    private BehaviorSubject<string> subjectDump;

    public Player(EmpireTheGame app)
    {
        this.app = app;
        units = new List<IUnit>();
        //cities = new List<City>();

        map = new FoggyMapElem[app.Map.SizeX, app.Map.SizeY];
        for (int y = 0; y < app.Map.SizeY; y++)
        {
            for (int x = 0; x < app.Map.SizeX; x++)
            {
                map[x, y] = new FoggyMapElem();
            }
        }

        //on construction, no armies are present anyway?
        RenderFoggy();

        subjectDump = new BehaviorSubject<string>(Dump());
        //subjectDump.OnNext(Dump());
        DumpObs = subjectDump.AsObservable();
    }

    public void RenderFoggy()
    {

        //from the data in the model, create foggy map model for player

        //just go trough all units and cities and update map

        /*
        foreach (var army in GetArmies())
        {
            RenderFoggyForArmy(army);
        }

        //explore around my cities as well
        foreach (var city in cities)
        {
            RenderFoggyForXY(city.x, city.y);
            //mark city as mine

            int playerIndex = 0; //?
            FoggyMap tileType = (FoggyMap)((int)FoggyMap.city + playerIndex);
            map[city.x, city.y].type = tileType;

            //search for the visible enemies 
        }
        */
    }

    public string Dump()
    {   
        var rez = new StringBuilder();
        for (int y = 0; y < app.Map.SizeY; y++)
        {
            for (int x = 0; x < app.Map.SizeX; x++)
            {
                FoggyMapElem type = map[x, y];
                rez.Append(type.Dump());
            }
            rez.AppendLine();
        }
        return rez.ToString();
    }

    internal bool IsDead()
    {
        //count cities and units
        return true;
    }

    public IObservable<string> DumpObs { get; set; }


    public void AddUnit(IUnit ar)
    {
        units.Add(ar);
    }

    public List<IUnit> GetArmies()
    {
        return units;
    }








} //end class Player
