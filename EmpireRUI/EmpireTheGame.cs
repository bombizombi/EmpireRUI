namespace EmpireRUI;

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


}

