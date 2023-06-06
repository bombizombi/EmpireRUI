namespace EmpireRUI;

public class EmpireTheGame
{

    //public Player[] players;
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

    public EmpireTheGame()
    {
        Map = MapHolder.Default;
    }


}

