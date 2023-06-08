using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Models
{
    public class Player
    {
        EmpireTheGame app;
        private List<IUnit> units;
        private List<City> cities;

        public FoggyMapElem[,] map;


        public Player(EmpireTheGame app)
        {
            this.app = app;
            units = new List<IUnit>();
            cities = new List<City>();

            map = new FoggyMapElem[app.map.sizeX, app.map.sizeY];
            for (int y = 0; y < app.map.sizeY; y++)
            {
                for (int x = 0; x < app.map.sizeX; x++)
                {
                    map[x, y] = new FoggyMapElem();
                }
            }

            //on construction, no armies are present anyway?
            RenderFoggy();
        }

        //private Map?  MapView?
        public void RenderFoggy()
        {

            //from the data in the model, create foggy map model for player

            //just go trough all units and cities and update map

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
        }

        public List<Loc> RenderFoggyForArmy(IUnit army)
        {
            return RenderFoggyForXY(army.X, army.Y);
        }

        public List<Loc> RenderFoggyForXY(int x, int y)
        {
            List<Loc> changedlocs = new List<Loc>();

            //TODO this no good, needs enemy armies as well, this will not see enemies
            //right now, we only see the terrain and cities

            var explore = (int dx, int dy) =>
            {
                MapType terrainMap = app.map.map[x + dx + (y + dy) * app.map.sizeX];
                FoggyMap tileType = FoggyMapElem.ConvertFromTerrain( terrainMap);
                //FoggyMap tileType = (int)app.map.map[x + dx + (y + dy) * app.map.sizeX];
                // - check neutral cities, check opponent cities
                // - check opponent armies
                if( tileType == FoggyMap.cityNeutral)
                {
                    //assume neutral
                    //tileType = FoggyMap.cityNeutral;  well, it is
                    //check if owned
                    if( CheckActualCityOwner( x+dx,y+dy, out int playerIndex))
                    {
                        tileType = (FoggyMap)((int)FoggyMap.city + playerIndex);
                    }
                }
                //TODO check opp armies

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


                    changedlocs.Add(new Loc { x = x + dx, y = y + dy });
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

            return changedlocs;
        }

        private bool CheckActualCityOwner(int x, int y, out int playerIndex)
            //return true if city is owned by anybody
        {
            //for each player, for each city
            for (int i = 0; i < app.players.Length; i++)
            {
                var player = app.players[i];
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
            //find city?
            City? freeCity = app.map.cities.Find(c => c.x == x && c.y == y);
            if( freeCity is null)
            {
                Debug.Assert(false, "adding a non-existant city.");
                return;
            }

            //should remove all production from city, so that it shows the dialog
            freeCity.ResetProduction();
            app.map.cities.Remove(freeCity);
            cities.Add(freeCity); //not so free anymore

            //conquer city

        }

        public City? FindCity(int x, int y)
        {
            City? city = cities.Find(c => c.x == x && c.y == y);
            return city;
        }

        public City? FindFreshlyConqueredCity()
        {
            City? city = cities.Find(c => c.production == ProductionEnum.uninitialized );
            return city;

        }

        public void AddUnit(IUnit ar)
        {
            units.Add(ar);
        }

        public List<IUnit> GetArmies()
        {
            return units;
        }

        public void DebugStatus(StringBuilder rez)
        {
            var a = ActiveUnit;
            if( a != null)
                rez.AppendLine($"active loc:{a.X},{a.Y} {a.DebugStatus()}");
            foreach( IUnit u in units)
            {
                //rez.AppendLine($"{u.Name} {u.IsInSentry() steps:{u.}");
                rez.AppendLine($"{u.DebugStatus()}");
            }
        }


        public IUnit? ActiveUnit; 
        public async Task<IUnit> ActivateUnit(FeedbackTasks tasks, IUnit? ignore = null)
        {
            //try to find a unit that did not move yet, also process standing orders while doing that
            //return null if there is no more armies that need attention

            ActiveUnit = null;
            //find first army still to move
            foreach (var u in units) //this loop should be in the view?
            {
                if (u.IsInSentry()) continue;

                if (u.StepsAvailable > 0)
                {
                    bool armyMoved = false;
                    //check for standing orders
                    if( u.StandingOrder != StandingOrders.None)
                    {
                        armyMoved = await HandleStandingOrders(u, tasks);
                    }

                    if (!armyMoved && (u != ignore))
                    {
                        ActiveUnit = u;
                        break;
                    }
                }
            }
            if( ActiveUnit == null)
            {
                //Debugger.Break();
            }
            return ActiveUnit;
        }



        private async Task<bool> HandleStandingOrders(IUnit u, FeedbackTasks tasks)
        {
            //return true if standing order caused this army to spend all its steps

            switch (u.StandingOrder)
            {
                case StandingOrders.LongGoto:
                    return await app.LongMoveStep(u, tasks);
                case StandingOrders.Explore:
                    return await app.ExploreStep(u, tasks);
                default:
                    Debugger.Break(); //unknown standing order
                    break;
            }
            return false;
        }


        public IUnit GetActiveArmy()
        {
            //find first army still to move
            foreach (var u in units)
            {
                if (u.StepsAvailable > 0)
                {
                    return u;
                }
            }
            return null; //end of move?
        }

        public async Task NewMove()
        {
            //TODO async feedback for city prod here
            foreach( var u in units)
            {
                u.NewTurn();
            }
            foreach (var c in cities)
            {
                c.NewTurn(this);
            }
        }

        public void UnitKilled(Army u)
        {
            units.Remove(u); 

        }

        public IUnit FriendlyContainerAtLoc(int x, int y)
        {
            foreach (var u in units)
            {
                if (!((u.GetType() == typeof(Transport)) || (y.GetType() == typeof(Carrier)))) continue;
                if (!((u.X == x) && (u.Y == y))) continue;
                return u;
            }
            return null;
        }



    }

    public class FoggyMapElem
    {
        //this si no good, foggy map must save everything visible (city owner, units from diff players)
        //cities will be 10 + owning player (9 for neutral city)

        public FoggyMap type; //?
        //int owner; implicity in the type
        public FoggyMapElem()
        {
            type = 0;
            //owner = -1;
        }

        public static FoggyMap ConvertFromTerrain(MapType terrainMap)
        {
            FoggyMap rez = (FoggyMap)terrainMap;
            if( terrainMap == MapType.city)
            {
                rez = FoggyMap.cityNeutral; //?
            }
            return rez;
            //city?

        }

        public static bool IsCity(FoggyMap map)
        {
            if ((map >= FoggyMap.cityNeutral) && (map <= FoggyMap.city + 9)) return true;
            return false;
        }

    }
}
