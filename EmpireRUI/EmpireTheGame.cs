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
        //DebugMoveRight();
        switch (tempMove2.type)
        {
            case GameOrder.Type.Move:
                DebugMoveDirection(tempMove2.x, tempMove2.y);
                break;
            default:
                Debug.Assert(false, "unknown game order type");
                break;
        }

    }

    public void DebugMoveDirection(int deltax, int deltay)
    {
        var army = Players.First().Units.First();
        MoveTo(army.X + deltax, army.Y + deltay, army);

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
        //can't go into water test
        //can't go out of map test



        //normal move means only neighbour cells, even for units with longer range

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


        ////this is no good.  Armies attack enemy and neutral cities, but enter their cities
        //if (type == MapType.city) return Move_HandleCity(army, x, y);



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

        ////check if it can hop on it
        //var ship = ActivePlayer.FriendlyContainerAtLoc(x, y);
        //if (ship != null) return Move_HandleLoading(army, x, y, ship);


        //check if it can bombard it


        //check unit if it likes it
        if (!army.CanStepOn(type)) return false;

        if (army.StepsAvailable == 0)
        {
            Debug.Assert(false, "why was unit with 0 steps active?");
            return false;
        }



        //222222222

        army.MoveTo(x, y);

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

