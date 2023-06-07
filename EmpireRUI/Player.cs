namespace EmpireRUI;

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






}
