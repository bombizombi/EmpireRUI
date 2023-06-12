namespace EmpireRUI;

public class GameOrder
{
    public int x;
    public int y;

    public Type type;

    public GameOrder(Type t, int x, int y)
    {
        this.type = t;
        this.x = x;
        this.y = y;
    }
    //        GameOrder order = new GameOrder(Gameorder.Type.MoveActive, p);


    public override string ToString()
    {
        return $"GameOrder: {type} xy:({x},{y})" ;
    }

    //create an enum type called Type
    public enum Type
    {
        Move,
        LongMove,
        Sentry,
        Unload,
        UnsentryAll,
        HackChangeCityProduction,
        HackHomeBaseForUnitProduction,
    }
}
