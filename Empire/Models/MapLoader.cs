using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace Empire.Models
{

    public class MapLoader
    {
        /* map format:  first 4 bytes, map size x and y, (reduced by 1)
         * map is run-length encoded, bits 0 and 1 are the cell type
         *   1 - city
         *   2 - sea
         *   3 - land
         * bits 2-7 are run length -1, (add 1 to get the length)
         * 
         * after the map data, there is a list of 16 bit numbers with a count of cities on the continet the city is on
         */


        public string fileName = "images/A.MAP";

        public int sizeX = 0;
        public int sizeY = 0;
        //public byte[] map;  //code like it's 1989.
        public MapType[] map;  
        public List<City> cities;

        public int NumberOfCells { get { return sizeX * sizeY; } }

        public MapLoader()
        {
            StringBuilder rez = new();
            string[] dbgdisplay = new string[] { "?", "X", ".", "o" };
            StringBuilder debugCitiesRez = new();



            using (var stream = File.Open(fileName, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.ASCII, false))
                {

                    //header
                    byte hbyte = reader.ReadByte();
                    byte lbyte = reader.ReadByte();
                    int sizeY = hbyte*256 + lbyte + 1;

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
                            if( type == MapType.city)
                            {
                                var c = new City(x, y);
                                cities.Add(c);
                            }

                            x++;
                            if( x>= sizeX)
                            {
                                x = 0;
                                y++;
                            }

                        }


                        //detect when map is done
                        if ( count == sizeX * sizeY)
                        {
                            //ignoring continent city counts for now
                            break;
                        }


                    }

                }
            }


        }

        public void DebugFakeCity()
        {
            int x = 2;
            int y = 4;
            int p = x + sizeX * y;

            map[p] = MapType.city;
            var c = new City(x, y);
            cities.Add(c);

        }


    } //end class MapLoader

    //MapTypeDisk
    //MapTypeBasic
    //MapTypeTerrain
    /* MapType - from actual map files, used for holding the actual map, and movemen and attack checks
     * FoggyMap - used for displaying, also for holding the actual foggy maps
     */

    public enum MapType : byte
    {
        //update FoggyMap enum as well
        unknown = 0,
        city = 1,
        sea = 2,
        land = 3,

    }

    public enum FoggyMap : byte
        /* foggy armies = base type + player owner
         * foggy cities = base city (10) + player owner, also 9 for neutral
         */
    {
        unknown = 0,
        //city = 1, ??
        sea = 2,
        land = 3,

        cityNeutral = 9,
        city = 10,

        army = 20,
        fighter = 30,
        transport = 40


    }

    /*        army = 0,
        figher,
        destroyer,
        transport,
        submarine,
        cruiser,
        carrier,
        battleship
*/
    public interface IDup
    {

    }

}
