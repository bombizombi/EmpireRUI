namespace EmpireRUI;

public interface IUnit 
{
    public int X { get;  }
    public int Y { get;  }
    public string Name { get; }
    public int StepsAvailable { get;}
    public int Hitpoints { get; }
    public int BaseFoggyType { get; }

    void MoveTo(int x, int y);

    public void NewTurn();
    public StandingOrders StandingOrder { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }


    //public bool IsVisible();
    public bool CanAttackIt(MapType type);
    public bool CanStepOn(MapType type);
    //public bool IsInSentry();

    public void EnterCity();
    public bool AttackCity();
    //public void LoadContainer(); //this unit enters a container
    //public void LoadUnit(IUnit u, int x, int y); //this containers gets a new passanger unit
    //public void UnloadUnit(IUnit u);
    //public void Unload();
    ////public void Explore();
    //public bool ShipContainsUnit(IUnit u);
    //public List<Loc> RenderFoggy();
    public void RenderFoggy();


    //public FoggyMap GetUnitType();
    public void Die();


    //public string DebugStatus();

    //22
    public void HackMoveAndReduceSteps(int dx, int dy);
}


public class Army : IUnit
{

    private int x;
    private int y;
    public Player player;
    public string name;

    private int stepsAvailable = 1;
    public int hitpoints;


    public StandingOrders standingOrder;
    public int targetX, targetY;
    private bool unitIsContained;

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

    public virtual void MoveTo(int x, int y)
    {
        //if (unitIsContained)
        //{
        //    unitIsContained = false; //more unloading processing?
        //}

        this.x = x;
        this.y = y;
        Debug.Assert(StepsAvailable > 0, "no steps available");
        stepsAvailable -= 1;

        //MoveContainedUnits(x, y);

        RenderFoggy();

        //TODO this code will have to be converted to observables 
        //var locs = RenderFoggy();
        //await tasks.Add(this, Tasks.StopBeingActive);
        //await tasks.Add(this, Tasks.DelayInbetweenSteps, locs);


    }
    public void HackMoveAndReduceSteps(int stepX, int stepY)
    {
        //this is kind of exactly the same as the one above
        x += stepX; //this is wrong
        y += stepY;
        stepsAvailable -= 1;
        //var locs = army.RenderFoggy();
        RenderFoggy();
    }

    public virtual void EnterCity()
    {
        //entering the city removes all steps, and resets unit range
        stepsAvailable = 0;
        unitIsContained = true;
    }



    public void NewTurn()
    {
        stepsAvailable = FullSteps();  //back to life
    }


    public void RenderFoggy()
    {
        //might return true if something of interest was discovered
        Debug.WriteLine("render foggy for army");
        this.player.RenderFoggyForArmy(this);
    }
    public virtual bool CanStepOn(MapType type) => type == MapType.land;

    public virtual bool CanAttackIt(MapType type) => type == MapType.city; //lol, we can attack more


    public bool AttackCity()
    {
        //armies only?
        if (rnd.NextDouble() < 0.5)  //TODO configurable probs?
        {
            hitpoints--;
            if (hitpoints == 0)
            {
                Debug.WriteLine("-------------------------------------------------------------City attacked and we lose.");
                Die();
                return false;
            }
        }
        Debug.WriteLine("-----------------------------00000000----------------------------City attacked and we win.");
        return true;
    }




    public void Die()
    {
        player.UnitKilled(this);
    }



    protected static int count = 0;
    public virtual int FullHitpoints() => 1;
    public virtual int FullSteps() => 1;

    internal void DebugCreateStandingOrder(StandingOrders order, int tx, int ty)
    {
        standingOrder = order;
        targetX = tx;
        targetY = ty;

    }

    public int X => x;
    public int Y => y;
    public string Name => name;

    public StandingOrders StandingOrder { get { return standingOrder; } set { standingOrder = value; } }
    public int TargetX { get { return targetX; } set { targetX = value; } }
    public int TargetY { get { return targetY; } set { targetY = value; } }

    public int StepsAvailable => stepsAvailable;
    public int Hitpoints => hitpoints;

    public int BaseFoggyType => (int)FoggyMap.army;

    public static IRandom rnd = new EmpireRandom();


} //end Army


public interface IRandom
{
    public double NextDouble();
}
public class EmpireRandom : IRandom
{
    public double NextDouble() => rnd.NextDouble();
    private static Random rnd = new Random();
}
public class RandomForTesting : IRandom
{
    private double[] probs;
    public RandomForTesting(double[] probs)
    {
        this.probs = probs;
    }
    public double NextDouble()
    {
        var p = probs[0];
        probs = probs.Skip(1).ToArray();
        return p;
    }
}

public enum StandingOrders
{
    None,
    LongGoto,
    Sentry,
    Explore,
}



