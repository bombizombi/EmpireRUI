using System.IO;
using System.Windows.Shapes;
//using System.Reflection.PortableExecutable;

namespace EmpireRUI;

public class MapHolder
{

    public int SizeX { get; set; }
    public int SizeY { get; set; }
    //public byte[] map;  //code like it's 1989.
    public MapType[] Map;
    public List<City> Cities { get; set; }


    public MapHolder(string mapString)
    {
        var lines = mapString.Split(Environment.NewLine);
        SizeX = lines[0].Length;
        SizeY = lines.Length;
        Map = new MapType[SizeX * SizeY];
        Cities = new List<City>();

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

                if (type == MapType.city)
                {
                    var c = new City(x, y);
                    Cities.Add(c);
                }

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


    /* map format:  first 4 bytes, map size x and y, (reduced by 1)
     * map is run-length encoded, bits 0 and 1 are the cell type
     *   1 - city
     *   2 - sea
     *   3 - land
     * bits 2-7 are run length -1, (add 1 to get the length)
     * 
     * after the map data, there is a list of 16 bit numbers with a count of cities on the continet the city is on
     */

    //public int NumberOfCells { get { return sizeX * sizeY; } }

    public void WhatAreThoseErrors()
    {

    }

    public void LoadMapFromFile()
    {
        var map = MapLoader_Load ();
        Map = map;

        SizeX = sizeX;
        SizeY = sizeY;
        Cities = cities;
    }

    public int sizeX = 0;
    public int sizeY = 0;
    public MapType[] map_old;
    public List<City> cities;

    int a123 = 123;


    public MapType[] MapLoader_Load()
    {
        StringBuilder rez = new();
        string[] dbgdisplay = new string[] { "?", "X", ".", "o" };
        StringBuilder debugCitiesRez = new StringBuilder();

        string fileName = "images/A.MAP";

        MapType[] map;


        using (var stream = File.Open(fileName, FileMode.Open))
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
            {

                //header
                byte hbyte = reader.ReadByte();
                byte lbyte = reader.ReadByte();
                int sizeY = hbyte * 256 + lbyte + 1;

                hbyte = reader.ReadByte();
                lbyte = reader.ReadByte();
                int sizeX = hbyte * 256 + lbyte + 1;

                //map = new byte[ sizeX * sizeY];  //code like it's 1989.
                map = new MapType[sizeX * sizeY];
                this.sizeX = sizeX;
                this.sizeY = sizeY;

                cities = new List<City>();


                //map body

                int p = 0;
                int count = 0;
                var inccheckcount = () =>
                {
                    count++;
                    if (count % 100 == 0)
                    {
                        rez.AppendLine();
                    }
                };


                int x = 0; int y = 0;

                while (true)
                {

                    byte a = reader.ReadByte();

                    //int type = a & 3;
                    MapType type = (MapType)(a & 3);
                    int length = (a >> 2) + 1;


                    if (type == MapType.city)
                    {
                        Debug.Assert(length == 1, "cities could be next to each other");
                        rez.Append(dbgdisplay[(int)type]);
                        //count++;
                        inccheckcount();

                        debugCitiesRez.AppendLine("len = " + length);
                    }
                    else
                    {

                        for (int i = 0; i < length; i++)
                        {
                            rez.Append(dbgdisplay[(int)type]);
                            inccheckcount();
                            //count++;
                        }
                    }



                    //up here is just debug string formating
                    for (int i = 0; i < length; i++)
                    {
                        map[p] = type;
                        p++;

                        //collect cities
                        if (type == MapType.city)
                        {
                            var c = new City(x, y);
                            cities.Add(c);
                        }

                        x++;
                        if (x >= sizeX)
                        {
                            x = 0;
                            y++;
                        }

                    }


                    //detect when map is done
                    if (count == sizeX * sizeY)
                    {
                        //ignoring continent city counts for now
                        break;
                    }
                }
            }
        }

        return map;

    }//end MapLoader_Load



}//end class MapHolder











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

            FoggyMap.transport => 't',

            FoggyMap.activeUnitFlasher => '!',
            //yy
            _ => '?'
        };
        if (c == '?') Debug.Assert(false, "unknown foggy map type");
        return c;
    }
} //end class FoggyMapElem





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

    activeUnitFlasher = 8, //will switch between unit char and active unit marker

    cityNeutral = 9,
    city = 10,  //city of player 0

    army = 20,
    fighter = 30,
    transport = 40

}



