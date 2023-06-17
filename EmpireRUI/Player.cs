using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.Pkcs;
using System.Security.Policy;

namespace EmpireRUI;

public class Player
{
    EmpireTheGame app;
    private List<IUnit> units;
    private List<City> cities;
    private int moveNumber = 0;

    public FoggyMapElem[,] map;

    private BehaviorSubject<string> subjectDump;
    private BehaviorSubject<string> subjectMessage;

    public int Index;

    public bool testDead = false;


    public IObservable<string> DumpObs { get; set; }
    public IObservable<string> MessageObs { get; set; }


    public Player(EmpireTheGame app)
    {
        this.app = app;
        units = new List<IUnit>();
        cities = new List<City>();

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
        var slowDump = subjectDump
            .AsObservable()
            .Select(x =>
                Observable
                    .Empty<string>()
                    .Delay(TimeSpan.FromSeconds(0.2))
                    .StartWith(x))
            .Concat()
            //.ObserveOn(RxApp.MainThreadScheduler)
            ;
        //DumpObs = subjectDump.AsObservable(); //this will put nondelayed obs as public

        DumpObs = slowDump;
        //expose normal dump as well for testing

        subjectMessage = new BehaviorSubject<string>("");
        MessageObs = subjectMessage.AsObservable();
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

#if ANONFUNCturnedooff
        var explore = (int dx, int dy) =>
        {
            if (x + dx < 0) return;
            if (y + dy < 0) return;
            if (x + dx >= app.Map.SizeX) return;
            if (y + dy >= app.Map.SizeY) return;

            MapType terrainMap = app.Map.Map[x + dx + (y + dy) * app.Map.SizeX];
            FoggyMap tileType = FoggyMapElem.ConvertFromTerrain(terrainMap);
            FoggyMap terrainMap_Foggy = tileType;  //copy original terrain
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

            //units can be contained in two different ways:
            //  -in cities, where they can stack unlimited
            //  -in transports, where there is a stack limit
            //when flashing for attention, normal unit switch between unit symbol and terrarin symbol
            //if they are contained, they switch between unit symbol and container symbol (transp or city or carrier)
            //
            //when exploring land, we need to only find one symbol. 

            var armies = from player in app.Players
                         from army in player.GetArmies()
                            where army.X == x + dx && army.Y == y + dy
                            where !army.IsContained || army.IsFlashing
                            select new { army, player };

            //there should be at most two armies, one top level container, one contained that is flashing

            if (armies.Count() == 1)
            {
                var el = armies.First();
                //Debug.WriteLine($"count is 1: {DebugDumpArmies(x + dx, y + dy)}");
                tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);
                if (!el.army.IsContained)
                {
                    if (el.army.IsFlashing)
                    {
                        //tileType = FoggyMap.activeUnitFlasher;  /this will flash "!" and "a"
                        tileType = terrainMap_Foggy; //this will flash "#" and "a"
                    }
                }
                if (el.army.IsContained)
                {
                    if (!el.army.IsFlashing)
                    {
                        //tileType = FoggyMap.activeUnitFlasher;  /this will flash "!" and "a"
                        tileType = terrainMap_Foggy; //this will flash "#" and "a"
                    }
                }
            } // end if 1

            //this is fine for containers that are trying to unload
            if (armies.Count() == 2)
            {
                Debug.WriteLine($"count is 1: {DebugDumpArmies(x + dx, y + dy)}");

                armies.ToList().ForEach(el =>
                {
                    if (el.army is Transport)
                    {
                        //Debugger.Break();
                    }

                    //tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);

                    Debug.WriteLine(DebugDumpArmies(x + dx, y + dy));

                    //if (!el.army.IsContained)   //if container
                    //{
                    //    if (el.army.IsFlashing)
                    //    {
                    //        tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);

                    //    }
                    //}
                    if (el.army.IsContained)
                    {
                        if (el.army.IsFlashing)
                        {
                            tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);
                        }
                    }



                });
                //22
            } //end if 2


            if (armies.Count() > 2)
            {
                string debugStringArmies = DebugDumpArmies(x+dx, y+dy);
                //Debug.WriteLine(debugStringArmies);
                //Debug.Assert(false, "trying to draw more than one army at the same spot");

            } 

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
#endif
        //update 8 squares
        //needs a edge safety check, or maybe not, edge columns should not be reachable

        ExploreSingleTile(x-1, y-1);
        ExploreSingleTile(x-1, y);
        ExploreSingleTile(x-1, y+1);
        ExploreSingleTile(x, y-1);
        ExploreSingleTile(x, y);
        ExploreSingleTile(x, y+1);
        ExploreSingleTile(x+1, y-1);
        ExploreSingleTile(x+1, y);
        ExploreSingleTile(x+1, y+1);

        //explore(-1, 0);
        //explore(-1, 1);
        //explore(0, -1);
        //explore(0, 0); // center needed?
        //explore(0, 1);
        //explore(1, -1);
        //explore(1, 0);
        //explore(1, 1);



        subjectDump.OnNext(Dump());
        //return changedlocs;
    }


    //111111111111
    private void ExploreSingleTile( int x, int y)
    { 
        //var explore = (int dx, int dy) =>
        if (x < 0) return;
        if (y < 0) return;
        if (x >= app.Map.SizeX) return;
        if (y >= app.Map.SizeY) return;

        MapType terrainMap = app.Map.Map[x + y * app.Map.SizeX];
        FoggyMap tileType = FoggyMapElem.ConvertFromTerrain(terrainMap);
        FoggyMap terrainMap_Foggy = tileType;  //copy original terrain
                                               //FoggyMap tileType = (int)app.map.map[x + dx + (y + dy) * app.map.sizeX];
                                               // - check neutral cities, check opponent cities
                                               // - check opponent armies
        if (tileType == FoggyMap.cityNeutral)
        {
            //assume neutral
            //tileType = FoggyMap.cityNeutral;  well, it is
            //check if owned
            if (CheckActualCityOwner(x, y, out int playerIndex))
            {
                tileType = (FoggyMap)((int)FoggyMap.city + playerIndex);
            }
        }
        //TODO check opp armies

        //foreach players
        //create linq querry that looks trough all players, and all the players armies
        //and if the armies coords match current xt, then do something

        //units can be contained in two different ways:
        //  -in cities, where they can stack unlimited
        //  -in transports, where there is a stack limit
        //when flashing for attention, normal unit switch between unit symbol and terrarin symbol
        //if they are contained, they switch between unit symbol and container symbol (transp or city or carrier)
        //
        //when exploring land, we need to only find one symbol. 

        var armies = from player in app.Players
                     from army in player.GetArmies()
                     where army.X == x && army.Y == y 
                     where !army.IsContained || army.IsFlashing
                     select new { army, player };

        //there should be at most two armies, one top level container, one contained that is flashing

        if (armies.Count() == 1)
        {
            var el = armies.First();
            //Debug.WriteLine($"count is 1: {DebugDumpArmies(x, y)}");
            tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);
            if (!el.army.IsContained)
            {
                if (el.army.IsFlashing)
                {
                    //tileType = FoggyMap.activeUnitFlasher;  /this will flash "!" and "a"
                    tileType = terrainMap_Foggy; //this will flash "#" and "a"
                }
            }
            if (el.army.IsContained)
            {
                if (!el.army.IsFlashing)
                {
                    //tileType = FoggyMap.activeUnitFlasher;  /this will flash "!" and "a"
                    tileType = terrainMap_Foggy; //this will flash "#" and "a"
                }
            }
        } // end if 1

        //this is fine for containers that are trying to unload
        if (armies.Count() == 2)
        {
            //Debug.WriteLine($"count is 1: {DebugDumpArmies(x, y)}");

            armies.ToList().ForEach(el =>
            {
                if (el.army is Transport)
                {
                    //Debugger.Break();
                }

                //tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);

                Debug.WriteLine(DebugDumpArmies(x, y));

                //if (!el.army.IsContained)   //if container
                //{
                //    if (el.army.IsFlashing)
                //    {
                //        tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);

                //    }
                //}
                if (el.army.IsContained)
                {
                    if (el.army.IsFlashing)
                    {
                        tileType = (FoggyMap)(el.army.BaseFoggyType + el.player.Index);
                    }
                }



            });
            //22
        } //end if 2


        if (armies.Count() > 2)
        {
            string debugStringArmies = DebugDumpArmies(x, y);
            //Debug.WriteLine(debugStringArmies);
            //Debug.Assert(false, "trying to draw more than one army at the same spot");

        }

        if (
                        //             map[x + dx, y + dy].type !=
                        //(int)app.map.map[x + dx + (y + dy) * app.map.sizeX])
                        map[x, y].type != tileType
            )
        {

            //FoggyMapElem 

            //record changed Locs
            //map[x + dx, y + dy].type = (int)app.map.map[x + dx + (y + dy) * app.map.sizeX];
            map[x, y].type = tileType;

            //who's city is it?


            //changedlocs.Add(new Loc { x = x + dx, y = y + dy });
        }


    }//end methond ExploreSingleTile



    public string DebugDumpArmies(int x, int y)
    {
        var rez = new StringBuilder();
        //armies from this player
        foreach (var army in units)
        {
            string active = (army == ActiveUnit) ? "ACTIVE" : "";
            rez.AppendLine($"at ({army.X,2},{army.Y,2}) army {army.Name} {active} ");
        }
        //armies from all the players

        rez.AppendLine();
        rez.AppendLine("now, units from all players");
        var allArmies = from player in app.Players
                     from army in player.GetArmies()
                     where army.X == x  && army.Y == y 
                     where !army.IsContained || army.IsFlashing
                     select new { army, player };
        foreach (var armyObj in allArmies)
        {
            var army = armyObj.army;
            string d = "";
            if (army.IsContained)
            {
                d = "contained ";
            }
            if (army.IsFlashing)
            {
                d += "flashing";
            }
            rez.AppendLine($"at ({army.X,-2},{army.Y,-2}) army {army.Name} {d}");
        }

        //where!army.IsContained || army.IsFlashing


        return rez.ToString();
    }

    public void RenderOnHearbeat(bool visible)
    {
        //race condition here, sometimes we have active unit null, even with a single army and no standing orders
        var au = ActiveUnit;
        if (au is null) return;
        au.SetFlashing(visible);

        //here flashing should get a pipe back out without the delay after
        RenderFoggyForArmy(au);
    }



    public City? FindCity(int x, int y)
    {
        City? city = cities.Find(c => c.x == x && c.y == y);
        return city;
    }

    private bool CheckActualCityOwner(int x, int y, out int playerIndex)
    {
        //return true if city is owned by anybody
        //for each player, for each city
        for (int i = 0; i < app.Players.Length; i++)
        {
            var player = app.Players[i];
            if (player.FindCity(x, y) is not null)
            {
                playerIndex = i;
                return true;
            }
        }
        playerIndex = -9999;
        return false;
    }

    public void AddCity(int x, int y)
    {
        //this does not handle enemy cities

        //find city?
        City? freeCity = app.Map.Cities.Find(c => c.x == x && c.y == y);
        if (freeCity is null)
        {
            Debug.Assert(false, "adding a non-existant city.");
            return;
        }

        //should remove all production from city, so that it shows the dialog
        freeCity.ResetProduction();
        app.Map.Cities.Remove(freeCity);
        cities.Add(freeCity); //not so free anymore

        //conquer city TODO

    }






    internal bool IsDead()
    {
        if (testDead) return true;
        return (!GetArmies().Any()) && (!GetCities().Any());
        //return true; //player dead
    }



    public void AddUnit(IUnit army, City homeCity=null)
    {
        army.SetHomeCity(homeCity);
        units.Add(army);

        //on adding, armies should reveal fog.  
        RenderFoggyForArmy(army);

    }

    public IUnit? ActiveUnit;
    //public async Task<IUnit> ActivateUnit(FeedbackTasks tasks, IUnit? ignore = null)
    public IUnit ActivateUnit( IUnit? ignore = null)
    {
        //try to find a unit that did not move yet, also process standing orders while doing that
        //return null if there is no more armies that need attention

        ActiveUnit = null;
        //find first army still to move
        foreach (var u in units) //this loop should be in the view?
        {
            //if (u.IsInSentry()) continue;

            if (u.StepsAvailable > 0)
            {
                bool armyMoved = false;
                //check for standing orders
                if (u.StandingOrder != StandingOrders.None)
                {
                    //armyMoved = await HandleStandingOrders(u, tasks);
                    //armyMoved could also be called "are all steps spent?"
                    //                               "is transporter fully loaded with steps left? -but opposite"
                    armyMoved = HandleStandingOrders(u);
                    subjectMessage.OnNext($"army {u.Name} executing standing order.");
                    Debug.WriteLine($"army {u.Name} executing standing order. {u}");
                }
                //logic here is wrong, if we first auto-move unit and then activate if there are still steps left
                //we never give MainGameLoop chance to create some delay between steps (so that user can actually
                //see what is happening)

                if (!armyMoved && (u != ignore)) // if not all steps spent
                {
                    ActiveUnit = u;
                    //subjectMessage.OnNext($"army {u.Name} active.");
                    subjectMessage.OnNext(u.DisplayActivationMessage);
                    break;
                }
            }
        }
        if (ActiveUnit == null)
        {
            //Debugger.Break();
        }
        return ActiveUnit;
    }

    //private async Task<bool> HandleStandingOrders(IUnit u, FeedbackTasks tasks)
    private bool HandleStandingOrders(IUnit u)
    {
        //return true if standing order caused this army to spend all its steps

        switch (u.StandingOrder)
        {
            case StandingOrders.LongGoto:
                //return await app.LongMoveStep(u, tasks);
                return app.LongMoveStep(u);
                break;
            case StandingOrders.Explore:
                Debug.Assert(false);
                //return await app.ExploreStep(u, tasks);
                break;
            case StandingOrders.Sentry:
                return SentryStep(u);
                break;
            case StandingOrders.Load:
                return app.LoadStep(u);
                break;
            default:
                Debugger.Break(); //unknown standing order
                break;
        }
        return false;
    }

    private bool SentryStep(IUnit u)
    {
        u.StepsAvailable = 0;
        //Sentry handling should be more complicated than this.  If you activate unit close to the end of turn
        //it should still have its steps left.  Right now, we will not be able to move it until next turn.
        //Also true for the armies inside transporters.
        return true; //senty step handled
    }


    public void NewMove()
    {
        moveNumber += 1;
        foreach (var unit in units)
        {
            unit.NewTurn();
        }
        foreach (var city in cities)
        {
            city.NewTurn(this);
        }
    }

    public int MoveNumber => moveNumber;

    public void UnitKilled(Army u)
    {
        units.Remove(u);
    }


    public IUnit? FriendlyContainerAtLoc(int x, int y)
    {
        //it should also check if it is not full
        foreach (var u in units)
        {
            //Debug.Assert(false); //turn on when Trasport is implemented
            if (!((u.GetType() == typeof(Transport)) || (y.GetType() == typeof(Carrier)))) continue;
            if (!((u.X == x) && (u.Y == y))) continue;

            if( u.IsFull) continue; //transport is full, cannot load more

            return u;
        }
        return null;
    }

    public IUnit? FriendlyUnitAtLoc(int x, int y)
    {
        foreach (var u in units)
        {
            //if (!((u.GetType() == typeof(Transport)) || (y.GetType() == typeof(Carrier)))) continue;
            if (!((u.X == x) && (u.Y == y))) continue;
            return u;
        }
        return null;
    }

    public IUnit? EnemyUnitAtLoc(int x, int y)
    {
        return app.EnemyUnitAtLocForPlayer(x, y, this);
    }




    public List<IUnit> GetArmies()
    {
        return units;
    }
    public List<City> GetCities()
    {
        return cities;
    }

    public IEnumerable<IUnit> GetUnitsAtLoc(int x, int y)
    {
        var units =
            from unit in GetArmies()
            where unit.X == x && unit.Y == y
            select unit;
        return units;
    }

    internal void Wait(IUnit armyToWait)
    {
        Debug.Assert(units.Contains(armyToWait), "Army to wait not in the list");
        units.Remove(armyToWait);
        units.Add(armyToWait);
    }

    public List<IUnit> Units => GetArmies();

    //public IUnit? ActiveUnit => null; //TODO


} //end class Player
