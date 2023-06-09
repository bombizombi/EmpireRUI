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
    //public bool CanAttackIt(MapType type);
    public bool CanStepOn(MapType type);
    //public bool IsInSentry();

    //public void EnterCity();
    //public bool AttackCity();
    //public void LoadContainer(); //this unit enters a container
    //public void LoadUnit(IUnit u, int x, int y); //this containers gets a new passanger unit
    //public void UnloadUnit(IUnit u);
    //public void Unload();
    ////public void Explore();
    //public bool ShipContainsUnit(IUnit u);
    //public List<Loc> RenderFoggy();
    public void RenderFoggy();


    //public FoggyMap GetUnitType();
    //public void Die();


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
        x += stepX; //this is wrong
        y += stepY;
        stepsAvailable -= 1;
        //var locs = army.RenderFoggy();
        RenderFoggy();
    }


    public void NewTurn()
    {
        stepsAvailable = FullSteps();  //back to life
    }


    public void RenderFoggy()
    {
        //might return true if something of interest was discovered
        this.player.RenderFoggyForArmy(this);
    }
    public virtual bool CanStepOn(MapType type) => type == MapType.land;



    protected static int count = 0;
    public virtual int FullHitpoints() => 1;
    public virtual int FullSteps() => 1;

    public int X => x;
    public int Y => y;
    public string Name => name;

    public StandingOrders StandingOrder { get { return standingOrder; } set { standingOrder = value; } }
    public int TargetX { get { return targetX; } set { targetX = value; } }
    public int TargetY { get { return targetY; } set { targetY = value; } }

    public int StepsAvailable => stepsAvailable;
    public int Hitpoints => hitpoints;

    public int BaseFoggyType => (int)FoggyMap.army;


} //end Army



public enum StandingOrders
{
    None,
    LongGoto,
    Sentry,
    Explore,
}



