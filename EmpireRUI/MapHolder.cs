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
        if (terrainMap == MapType.city)
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

    public char Dump()
    {
        char c = type switch
        {
            FoggyMap.unknown => ' ',
            FoggyMap.sea => '.',
            FoggyMap.land => 'o',
            FoggyMap.cityNeutral => '#',
            FoggyMap.city => '1',  //city of player 0

            FoggyMap.army => 'a',

            _ => '?'
        };
        if (c == '?') Debug.Assert(false, "unknown foggy map type");
        return c;
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


public enum FoggyMap : byte
/* foggy armies = base type + player owner zero based
 * foggy cities = base city (10) + player owner, also 9 for neutral
 */
{
    unknown = 0,
    //city = 1, ??
    sea = 2,
    land = 3,

    cityNeutral = 9,
    city = 10,  //city of player 0

    army = 20,
    fighter = 30,
    transport = 40

}



