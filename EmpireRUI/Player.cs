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

    public int Index;

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

//    public List<Loc> RenderFoggyForArmy(IUnit army)
    public void RenderFoggyForArmy(IUnit army)
    {
        //return 
        RenderFoggyForXY(army.X, army.Y);
    }


    //public List<Loc> RenderFoggyForXY(int x, int y)
    public void RenderFoggyForXY(int x, int y)
    {
        //RenderFoggyForXY used to return a list of changed Locs, but, it was not used before
        //and also, when this becomes needed, it will pass them trough an observable
        //right now we will just dump the whole text map trough the dumpObservable

        //runs in n**2 on number of units

        //List<Loc> changedlocs = new List<Loc>();

        //TODO this no good, needs enemy armies as well, this will not see enemies
        //right now, we only see the terrain and cities

        var explore = (int dx, int dy) =>
        {
            if (x + dx < 0) return;
            if (y + dy < 0) return;
            if (x + dx >= app.Map.SizeX) return;
            if (y + dy >= app.Map.SizeY) return;

            MapType terrainMap = app.Map.Map[x + dx + (y + dy) * app.Map.SizeX];
            FoggyMap tileType = FoggyMapElem.ConvertFromTerrain(terrainMap);
            //FoggyMap tileType = (int)app.map.map[x + dx + (y + dy) * app.map.sizeX];
            // - check neutral cities, check opponent cities
            // - check opponent armies
            if (tileType == FoggyMap.cityNeutral)
            {
                //assume neutral
                //tileType = FoggyMap.cityNeutral;  well, it is
                //check if owned
                if (CheckActualCityOwner(x + dx, y + dy, out int playerIndex))
                {
                    tileType = (FoggyMap)((int)FoggyMap.city + playerIndex);
                }
            }
            //TODO check opp armies

            //foreach players
            //create linq querry that looks trough all players, and all the players armies
            //and if the armies coords match current xt, then do something
            var armies = from player in app.Players
                         from army in player.GetArmies()
                            where army.X == x + dx && army.Y == y + dy
                            select new { army, player };
            Debug.Assert(armies.Count() <= 1);

            armies.ToList().ForEach(x =>

                tileType = (FoggyMap)(x.army.BaseFoggyType + x.player.Index)

                    //FoggyMap tileType = FoggyMapElem.ConvertFromTerrain(terrainMap);
                    //tileType = (FoggyMap)((int)FoggyMap.city + playerIndex);

            );
            //app.Players


            if (
                            //             map[x + dx, y + dy].type !=
                            //(int)app.map.map[x + dx + (y + dy) * app.map.sizeX])
                            map[x + dx, y + dy].type != tileType
                )
            {

                //FoggyMapElem 

                //record changed Locs
                //map[x + dx, y + dy].type = (int)app.map.map[x + dx + (y + dy) * app.map.sizeX];
                map[x + dx, y + dy].type = tileType;

                //who's city is it?


                //changedlocs.Add(new Loc { x = x + dx, y = y + dy });
            }


        };

        //update 8 squares
        //needs a edge safety check, or maybe not, edge columns should not be reachable

        explore(-1, -1);
        explore(-1, 0);
        explore(-1, 1);
        explore(0, -1);
        explore(0, 0); // center needed?
        explore(0, 1);
        explore(1, -1);
        explore(1, 0);
        explore(1, 1);


        subjectDump.OnNext(Dump());
        //return changedlocs;
    }

    private bool CheckActualCityOwner(int v1, int v2, out int playerIndex)
    {
        throw new NotImplementedException();
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
        return true; //player dead
    }

    public IObservable<string> DumpObs { get; set; }


    public void AddUnit(IUnit army)
    {
        units.Add(army);

        //on adding, armies should reveal fog.  
        RenderFoggyForArmy(army);

    }

    public List<IUnit> GetArmies()
    {
        return units;
    }








} //end class Player
