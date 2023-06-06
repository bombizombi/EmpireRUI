using System.Reflection.PortableExecutable;

namespace EmpireRUI;

public class MapHolder
{

    public int SizeX { get; set; }
    public int SizeY { get; set; }
    //public byte[] map;  //code like it's 1989.
    public MapType[] Map;
    //public List<City> Cities {get;set}


    public MapHolder(string mapString)
    {
        var lines = mapString.Split(Environment.NewLine);
        SizeX = lines[0].Length;
        SizeY = lines.Length;
        Map = new MapType[SizeX * SizeY];

        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                MapType type = lines[y][x] switch
                {
                    '.' => MapType.sea,
                    'o' => MapType.land,
                    '#' => MapType.city,
                    _ => MapType.unknown
                };
                Map[x + y * SizeX] = type;
            }
        }





    }


    public static MapHolder Default => new MapHolder("#o.o#");

    public string Dump()
    {
        var rez = new StringBuilder();
        for (int y = 0; y < SizeY; y++)
        {
            for (int x = 0; x < SizeX; x++)
            {
                MapType type = Map[x + y * SizeX];
                char c = type switch
                {
                    MapType.sea => '.',
                    MapType.land => 'o',
                    MapType.city => '#',
                    _ => '?'
                };
                rez.Append(c);
            }
            rez.AppendLine();
        }
        return rez.ToString();
    }
}


public enum MapType : byte
{
    //update FoggyMap enum as well
    unknown = 0,
    city = 1,
    sea = 2,
    land = 3,

}


