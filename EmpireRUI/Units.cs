using System.Xml.Linq;

namespace EmpireRUI;

public interface IUnit 
{
    public int X { get;  }
    public int Y { get;  }
    public string Name { get; }
    public int StepsAvailable { get; }
    public int Hitpoints { get; }
    public int BaseFoggyType { get; }

}


public class Army : IUnit
{

    private int x;
    private int y;
    public Player player;
    public string name;

    private int stepsAvailable = 1;
    public int hitpoints;

    //public StandingOrders standingOrder;
    //public int targetX, targetY;
    //private bool unitIsContained;
    public Army(int x, int y, Player p, int stepsLeft=-1, int hitPointsLeft = -1)
    {
        Army.count++;

        this.x = x;
        this.y = y;
        stepsAvailable = stepsLeft == -1 ? FullSteps() : stepsLeft;
        hitpoints = hitPointsLeft == -1 ? FullHitpoints() : hitPointsLeft;


        player = p;
        name = "Army no " + Army.count;

    }
    protected static int count = 0;
    public virtual int FullHitpoints() => 1;
    public virtual int FullSteps() => 1;

    public int X => x;
    public int Y => y;
    public string Name => name;
    public int StepsAvailable => stepsAvailable;
    public int Hitpoints => hitpoints;

    public int BaseFoggyType => (int)FoggyMap.army;


} //end Army

