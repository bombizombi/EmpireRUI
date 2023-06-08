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



}

