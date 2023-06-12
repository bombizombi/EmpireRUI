using System.Xaml;

namespace EmpireRUI;

/*General class descriptions and responsibilities
 * 
 * EmpireTheGame: 
 *      -will hold the static terrain map
 *      -contains players
 *      
 * Player
 *      -will hold the fog of war map 
 *      -will hold units and cities belonging to the player
 *      
 *      
 * Design decisions:
 *      Main game loop, I put it in the MapViewModel, but maybe it belongs to the EmpireTheGame?
 *      
 * 
 */

public class EmpireTheGame
{

    public Player[] Players;
    public int SizeX => Map.SizeX;
    public int SizeY => Map.SizeY;
    public MapHolder Map { get; set; }


    public static EmpireTheGame FromString(string map)
    {
        var empire = new EmpireTheGame();
        empire.Map = new MapHolder(map);
        return empire;
    }

    public string Dump()
    {
        return Map.Dump();
    }

    
    public EmpireTheGame() : this(MapHolder.Default)
    {
        //Map = MapHolder.Default;
    }

    public EmpireTheGame(MapHolder map) : this(map , 2)
    {
        //Map = map;
    }
    public EmpireTheGame(MapHolder map, int playerCount = 2)
    {
        Map = map;
        Players = new Player[playerCount];
    }

    public EmpireTheGame( string map, int playerCount = 2)
    {
        Map = new MapHolder(map);
        Players = new Player[playerCount];
    }

    public EmpireTheGame(string map, int playerCount = 2, string filename=null)
    {
        Map = new MapHolder(map);
        Players = new Player[playerCount];
        if( filename != null)
        {
            Map.LoadMapFromFile();

        }
    }

    public void PickRandomCity()
    {
        City city = Map.Cities[0];
        ActivePlayer.AddCity(city.x, city.y);
    }




    public Player AddPlayer()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i] == null)
            {
                var player = new Player(this);
                player.Index = i;
                Players[i] = player;
                return player;
            }
        }
        Debug.Assert(false, "No more players can be added");
        return null;

    }

    public Player ActivePlayer => Players[0];




    internal void GameMove(GameOrder tempMove2)
    {
        ActivePlayer.ActiveUnit.SetFlashing(false);

        //DebugMoveRight();
        switch (tempMove2.type)
        {
            case GameOrder.Type.Move:
                OrderMoveDirection(tempMove2.x, tempMove2.y);
                break;
            case GameOrder.Type.LongMove:
                OrderLongMoveTo(tempMove2.x, tempMove2.y);
                break;
            case GameOrder.Type.Sentry:
                OrderSentry();
                break;
            case GameOrder.Type.Unload:
                OrderUnload();
                break;
            case GameOrder.Type.UnsentryAll:
                OrderUnsentryAll();
                break;
            case GameOrder.Type.HackChangeCityProduction:
                HackChangeCityProduction();
                break;
            case GameOrder.Type.HackHomeBaseForUnitProduction:
                HackChangeCityProduction();
                break;
            default:
                Debug.Assert(false, "unknown game order type");
                break;
        }

    }

    public void OrderMoveDirection(int deltax, int deltay)
    {
        //var army = Players.First().Units.First();
        var army = Players.First().ActiveUnit;
        MoveTo(army.X + deltax, army.Y + deltay, army);
    }
    public void OrderLongMoveTo(int deltax, int deltay)
    {
        var army = Players.First().ActiveUnit; 
        MoveTo(army.X + deltax, army.Y + deltay, army);
    }

    public void OrderSentry()
    {
        var army = ActivePlayer.ActiveUnit;
        if (army == null) return;

        //tasks.Add(army, Tasks.StopBeingActive);
        //TODO feedback on sentry
        army.StandingOrder = StandingOrders.Sentry;
        army.Sentry();
    }
    public void OrderUnload()
    {
        var army = ActivePlayer.ActiveUnit;
        if (army == null) return;
        army.Unload();
        
    }
    private void OrderUnsentryAll()
    {
        foreach (var army in ActivePlayer.Units)
        {
            if( army.StandingOrder == StandingOrders.Sentry)
            {
                army.StandingOrder = StandingOrders.None;
            }
        }
    }

    private void HackChangeCityProduction()
    {
        var cities = ActivePlayer.GetCities();

        //can you create a query that will sort by remaining descending?
        var city = cities
            .Where( c => !c.ChangeRequest )
            .OrderBy(c => c.remaining).FirstOrDefault();
        if (city == null) return;

        city.ChangeRequest = true;
    }


    private void HackHomeBaseProduction()
    {
        var city = ActivePlayer.ActivateUnit().HomeCity;
        if (city == null) return;
        city.ChangeRequest = true;
    }



    public void DebugMoveRight()
    {
        var army = Players.First().Units.First();
        MoveTo( army.X + 1, army.Y, army);
    }

    public bool DebugMoveInDirection(int dx=0, int dy = 0)
    {
        //dx and dy can be -1, 0, 1

        var army = Players.First().Units.First();
        return MoveTo(army.X +dx, army.Y + dy, army);
    }




    public bool MoveTo(int x, int y, IUnit? army =null)
    {
        //normal move means only neighbour cells, even for units with longer range

        //currenty, armies that attack and die do not update foggy map

        //can only move to land close by, and can't step on the edge
        if (!CheckMapEdges(x, y))
        {
            Debug.WriteLine("can't move out of the map edge");
            return false;
        }

        army = army ?? ActivePlayer.ActiveUnit;

        //having no active army should be a normal state
        //add potential other action for clicks here
        if (army == null) return false;
        //Debug.Assert( army != null , "trying to move, but no unit is active");   
        /*


        if (Math.Abs(y - army.Y) > 1) return false;
        if (Math.Abs(x - army.X) > 1) return false;

        bool doNotAllowGoingNowhere = (army.X == x) && (army.Y == y);
        if (doNotAllowGoingNowhere) return false;
        */


        //get land type
        MapType type = Map.Map[x + y * Map.SizeX];


        //this is no good.  Armies attack enemy and neutral cities, but enter their cities
        if (type == MapType.city) return Move_HandleCity(army, x, y);

        //type here has not enough information
        /*
                    //check unit if it hates it
                    if(army.CanAttackIt(type))
                    {
                        //well, attack it is

                        //attack on a city?

                        //army.Attack( ?);
                        army.AttackCity();

                        //fight visaul feedback

                        //? yeah, but what about enemy units
                        Debugger.Break();

                    }*/

        //check if it can hop on it
        var ship = ActivePlayer.FriendlyContainerAtLoc(x, y);
        if (ship != null) return Move_HandleLoading(army, x, y, ship);

        //check if target loc is already occupied
        var friend = ActivePlayer.FriendlyUnitAtLoc(x, y);
        if (friend != null) return false; //here we can make some push for room logic?


        //check if it can bombard it

        //check unit if it likes it
        if (!army.CanStepOn(type)) return false;

        return MoveTo_Impl(army, x, y);

    }

    private bool MoveTo_Impl(IUnit army, int x, int y)
    {
        //this will actually do the moving
        //should be called from normal MoveTo, and from HandleCity
        if (army.StepsAvailable == 0)
        {
            Debug.Assert(false, "why was unit with 0 steps active?");
            return false;
        }

        army.MoveTo(x, y);

        //get army out of the container ship
        foreach (var sh in ActivePlayer.GetArmies())
        {
            if (sh.ShipContainsUnit(army))
            {
                sh.UnloadUnit(army);
                army.Unload();
                break;
            }
        }

        return false;
    }


#if oldMOVEasdfasd
    public async Task<bool> MoveTo(int x, int y, FeedbackTasks tasks)
    {
        //normal move means only neighbour cells, even for units with longer range

        //can only move to land close by, and can't step on the edge
        if (!CheckMapEdges(x, y)) return false;

        var army = ActivePlayer.ActiveUnit;

        //having no active army should be a normal state
        //add potential other action for clicks here
        if (army == null) return false;
        //Debug.Assert( army != null , "trying to move, but no unit is active");   


        if (Math.Abs(y - army.Y) > 1) return false;
        if (Math.Abs(x - army.X) > 1) return false;

        bool doNotAllowGoingNowhere = (army.X == x) && (army.Y == y);
        if (doNotAllowGoingNowhere) return false;




        //get land type
        MapType type = map.map[x + y * map.sizeX];

        //this is no good.  Armies attack enemy and neutral cities, but enter their cities

        if (type == MapType.city) return Move_HandleCity(army, x, y);



        //type here has not enough information
        /*
                    //check unit if it hates it
                    if(army.CanAttackIt(type))
                    {
                        //well, attack it is

                        //attack on a city?

                        //army.Attack( ?);
                        army.AttackCity();

                        //fight visaul feedback

                        //? yeah, but what about enemy units
                        Debugger.Break();

                    }*/

        //check if it can hop on it
        var ship = ActivePlayer.FriendlyContainerAtLoc(x, y);
        if (ship != null) return Move_HandleLoading(army, x, y, ship);


        //check if it can bombard it


        //check unit if it likes it
        if (!army.CanStepOn(type)) return false;

        if (army.StepsAvailable == 0)
        {
            Debug.Assert(false, "why was unit with 0 steps active?");
            return false;
        }


        await army.MoveTo(x, y, tasks);



        //bool isRiding = false; supress warning
        foreach (var sh in ActivePlayer.GetArmies())
        {
            if (sh.ShipContainsUnit(army))
            {
                sh.UnloadUnit(army);
                army.Unload();
                break;
            }
        }

        return true;
    }

    //222222222222222222222222222222222222
#endif




    /* long move command, coppied but not coverted

    //11
    public async Task<bool> LongMoveTo(int x, int y, FeedbackTasks tasks)
    {



        //long move:
        //           -save target cell
        //           -find path (major pita)
        //           -do moves up to unit steps left

        if (!CheckMapEdges(x, y)) return false; ;

        var army = ActivePlayer.ActiveUnit;

        if (army == null) return false;

        //long move
        //if (Math.Abs(y - army.y) > 1) return false;
        //if (Math.Abs(x - army.x) > 1) return false;
        if ((army.X == x) && (army.Y == y)) return false;
        //also do not alow going nowhere



        //validate?

        //get land type
        MapType type = map.map[x + y * map.sizeX];

        //might be able to send it into unknown land type
        //for now, leave checks the same as for a single step

        //check unit if it likes it
        if (!army.CanStepOn(type)) return false;

        //checks passed, save target cell
        army.StandingOrder = StandingOrders.LongGoto;
        army.TargetX = x;
        army.TargetY = y;

        //any actual move will be done in the next turn as well.

        //terrain check should be moved to the steps part from the init part

        //extract actual long step
        await LongMoveStep(army, tasks);


        await CheckEndOfTurn(tasks);

        //end extract actual long step

        return true;
    }
    */


    private bool Move_HandleCity(IUnit u, int x, int y)
    {
        //return value not used before


        City? city = ActivePlayer.FindCity(x, y);
        bool cityOwnedByPlayer = city != null;
        if (cityOwnedByPlayer)
        {
            //u.X = x;
            //u.Y = y;
            //u.StepsAvailable -= 1;
            //u.HackMoveAndReduceSteps(x , y ); //bug
            /*
            u.EnterCity(); // do not render
            u.HackMoveAndReduceSteps(x - u.X, y - u.Y);
            //hack move also renders 

            //Debugger.Break(); //stop here and check who does rendering in this case
            //the city does
            //enter the city
            u.EnterCity(); //we enter the city too late, its already rendered
            */
            MoveTo_Impl(u, x, y);

            u.EnterCity(); // be contained in the city, also lose all movements left

            //now, move all contained units to the city and wake them up


            return true;
        }


        //check if unit hates it
        if (u.CanAttackIt(MapType.city))
        {
            //well, attack it is

            //attack on a city?

            bool cityAttackWon = u.AttackCity();
            if (cityAttackWon)
            {
                //remove from previous owner, also handle units inside TODO
                ActivePlayer.AddCity(x, y);
                //winning army spreads around the city and lives a long peaceful life
            }
            else
            {
                //Debugger.Break();
                //losing army just dies
            }
            u.Die();  //turns out u.AttackCity already kills the unit
                      //but your death was not in vain, you get so see a little bit of map around the city
            ActivePlayer.RenderFoggyForXY(x,y);


            //fight visaul feedback?

            //

            //? yeah, but what about enemy units
            //Debugger.Break();

        }
        return true;

    }

    private bool Move_HandleLoading(IUnit u, int x, int y, IUnit ship)
    {
        //only ships to transports and planes to carriers
        bool caseArmyToShip = (u.GetType() == typeof(Army)) && (ship.GetType() == typeof(Transport));
        bool casePlaneToCarrier = (u.GetType() == typeof(Fighter)) && (ship.GetType() == typeof(Carrier));

        if (!(caseArmyToShip || casePlaneToCarrier)) return false;

        ship.LoadUnit(u, x, y);

        ActivePlayer.RenderFoggyForXY(x, y);
        return true;
    }

    //public async Task<bool> LongMoveStep(IUnit army, FeedbackTasks tasks)
    public bool LongMoveStep(IUnit army)
    {

        //await tasks.Add(army, Tasks.DelayBeforeMove);
        //Debugger.Break(); //see how we are going to create this delay

        do
        {

            int deltax = army.TargetX - army.X;
            int deltay = army.TargetY - army.Y;
            //TODO this dissregards terrain or any armies in the way

            Debug.Assert(!((deltax == 0) && (deltay == 0)), "long goto with no deltas");

            //trivial path finding
            int stepX = 0, stepY = 0;
            if (deltax != 0) stepX = deltax / Math.Abs(deltax); //unit vector in direction of target
            if (deltay != 0) stepY = deltay / Math.Abs(deltay); //unit vector in direction of target


            if (army.StepsAvailable == 0)
            {
                //await tasks.Add(army, Tasks.DelayAfterMove);
                Debugger.Break(); //see how we are going to create this delay
                return false;
            }

            //long move step behaves differently, it does not attack, it does not load
            //but does it enter cities?  Since it looses steps, perhaps it should avoid entering cities if possible


            //this might step onto a city or an emety unit, without attacking, without entering
            army.HackMoveAndReduceSteps(stepX, stepY);
            //army.X += stepX; //this is wrong
            //army.Y += stepY;
            //army.StepsAvailable -= 1;
            ////var locs = army.RenderFoggy();
            //army.RenderFoggy();


            //await tasks.Add(army, Tasks.DelayInbetweenSteps, locs);
            //Debugger.Break(); //see how we are going to create this delay

            //target reached?
            if ((army.X == army.TargetX) && (army.Y == army.TargetY))
            {
                army.StandingOrder = StandingOrders.None;
                //clear target coordinates
                army.TargetX = -1;
                army.TargetY = -1;
                //await tasks.Add(army, Tasks.DelayAfterMove);
                //Debugger.Break(); //see how we are going to create this delay
                break;
            }

        } while (army.StepsAvailable > 0);

        return army.StepsAvailable == 0;

    }



    //22222222




    private bool CheckMapEdges(int x, int y)
    {
        //can only move to land close by, and can't step on the edge
        //with the new bounds checking, the original maps will have a strange water around the edges
        if (x < 0) return false;
        if (y < 0) return false;
        if (x > Map.SizeX - 1) return false;
        if (y > Map.SizeY - 1) return false;
        //if (x == 0) return false;
        //if (y == 0) return false;
        //if (x == map.sizeX - 1) return false;
        //if (y == map.sizeY - 1) return false;
        return true;
    }



}//end class EmpireTheGame

