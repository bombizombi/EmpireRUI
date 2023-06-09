﻿using System.Xml.Linq;

namespace EmpireRUI;

public interface IUnit
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; }
    public int StepsAvailable { get; set; }
    public int Hitpoints { get; }
    public int BaseFoggyType { get; }

    public void MoveTo(int x, int y);
    public void Sentry();
    public void SkipMove();
    public void CreateStandingOrder(StandingOrders o, int x, int y);


    public void NewTurn();
    public StandingOrders StandingOrder { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }


    //public bool IsVisible();
    public bool CanAttackIt(MapType type);
    public bool CanPickUp(IUnit? unit);
    public bool CanStepOn(MapType type);
    //public bool IsInSentry();

    public void EnterCity(City city);
    public bool AttackCity();
    public void LoadContainer(); //this unit enters a container
    public void LoadUnit(IUnit u, int x, int y); //this container gets a new passanger unit
    public void UnloadUnit(IUnit u);
    public void Unload();
    public void Load();
    public bool IsFull { get; }
    ////public void Explore();
    public bool ShipContainsUnit(IUnit u);
    public bool IsContained { get; }

    public int LoadedUnitsCount { get; }
    public int Capacity{ get; }

    //public List<Loc> RenderFoggy();
    public void RenderFoggy();


    //public FoggyMap GetUnitType();
    public void Die();


    //public string DebugStatus();

    //22
    public void HackMoveAndReduceSteps(int dx, int dy);
    void SetFlashing(bool visible);
    public void ContainBrandNewUnit();

    public bool IsFlashing { get; }

    public void SetHomeCity(City city);
    public City HomeCity { get; }

    public string DisplayActivationMessage { get; }
}


//add debugger display attribute
[DebuggerDisplay("{Name} ({X},{Y}) s:{StepsAvailable} h:{Hitpoints} {IsContained}")]
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

    private City homeCity;

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
        if (unitIsContained)
        {
            unitIsContained = false; //more unloading processing?
        }

        this.x = x;
        this.y = y;
        Debug.Assert(StepsAvailable > 0, "no steps available");
        stepsAvailable -= 1;

        this.isFlashing = false; //should be always safe

        MoveContainedUnits(x, y);

        RenderFoggy();

        //TODO this code will have to be converted to observables 
        //var locs = RenderFoggy();
        //await tasks.Add(this, Tasks.StopBeingActive);
        //await tasks.Add(this, Tasks.DelayInbetweenSteps, locs);


    }

    public void Sentry()
    {
        standingOrder = StandingOrders.Sentry;
        SetFlashing(false);
        RenderFoggy();
    }

    public void SkipMove()
    {
        stepsAvailable = 0;
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

    protected virtual void MoveContainedUnits(int x, int y) {}


    public virtual void EnterCity(City? city=null)
    {
        //entering the city removes all steps, and resets unit range
        stepsAvailable = 0;
        unitIsContained = true;
    }

    public virtual void LoadUnit(IUnit u, int x, int y) //this container gets a new passanger unit
    {
        Debug.Assert(false, "Not a container.");
    }
    public virtual void UnloadUnit(IUnit u)
    {
        Debug.Assert(false, "not a container");
    }

    public virtual bool ShipContainsUnit(IUnit u) => false;


    public void LoadContainer()
    {
        standingOrder = StandingOrders.Sentry;
        unitIsContained = true;
    }

    public virtual void Unload() { }
    public virtual void Load() { }


    public bool IsContained => unitIsContained;

    public void NewTurn()
    {
        stepsAvailable = FullSteps();  //back to life
    }


    public void RenderFoggy()
    {
        //might return true if something of interest was discovered
        //Debug.WriteLine("render foggy for army");
        this.player.RenderFoggyForArmy(this);
    }
    public virtual bool CanStepOn(MapType type) => type == MapType.land;

    public virtual bool CanAttackIt(MapType type) => type == MapType.city; //lol, we can attack more

    public virtual bool CanPickUp(IUnit? unit)
    {
        Debug.Assert( false, "An army is not a container");
        return false;
    }


    public bool AttackCity()
    {
        //armies only?
        var dbl = rnd.NextDouble();
        Debug.WriteLine($"Attack city, random double = {dbl}" );
        if (dbl < 0.5)  //TODO configurable probs?
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



    public void ContainBrandNewUnit()
    {
        unitIsContained = true;
    }

    public void Die()
    {
        player.UnitKilled(this);
    }

    public void SetHomeCity(City city)
    {
        //used to invoke production dialog on any army
        homeCity = city;
    }
    public City HomeCity => homeCity;





    protected static int count = 0;
    public virtual int FullHitpoints() => 1;
    public virtual int FullSteps() => 1;
    public virtual int Capacity => 0;
    public virtual int LoadedUnitsCount => 0;


    public virtual void CreateStandingOrder(StandingOrders order, int tx, int ty)
    {
        standingOrder = order;
        targetX = tx;
        targetY = ty;
    }

    public int X { get { return x; } set { x = value; } }
    public int Y { get { return y; } set { y = value; } }
    public string Name => name;

    public StandingOrders StandingOrder { get { return standingOrder; } set { standingOrder = value; } }
    public int TargetX { get { return targetX; } set { targetX = value; } }
    public int TargetY { get { return targetY; } set { targetY = value; } }

    public int StepsAvailable { get { return stepsAvailable;  } set { stepsAvailable = value; } }
    public int Hitpoints => hitpoints;


    public virtual int BaseFoggyType => (int)FoggyMap.army;

    public static IRandom rnd = new EmpireRandom();

    public virtual bool IsFull => true;
    public void SetFlashing( bool visible)
    {
        isFlashing = visible;
        //?RenderFoggyForArmy(au);
    }
    public bool IsFlashing => isFlashing;
    protected bool isFlashing;

    public virtual string DisplayActivationMessage => $"army {Name} active.";




} //end Army





public class Transport : Army
{
    private int capacity;
    //private int loaded;

    public List<IUnit> loadedUnits;

    public Transport(int x, int y, Player p) : base(x, y, p)
    {

        name = "Transport no " + Army.count;

        capacity = Capacity;
        loadedUnits = new List<IUnit>();

    }


    protected override void MoveContainedUnits(int x, int y)
    {
        foreach (var u in loadedUnits)
        {
            u.X = x;
            u.Y = y;
        }
    }
    public override bool ShipContainsUnit(IUnit search)
    {
        foreach (var u in loadedUnits)
        {
            if (u == search) return true;
        }
        return false;
    }

    public override void LoadUnit(IUnit u, int x, int y)
    {
        Debug.Assert(!loadedUnits.Contains(u), "Unit is already inside.");

        loadedUnits.Add(u);
        u.X = x;
        u.Y = y;
        u.StepsAvailable = 0;
        u.SetFlashing(false);

        u.LoadContainer();

    }

    public override void EnterCity(City? city=null)
    {
        base.EnterCity(city);

        foreach (var u in loadedUnits)
        {
            //loadedUnits keep their steps, and their "contained" status
            //they also wake up
            if(u.StandingOrder == StandingOrders.Sentry)
            {
                u.StandingOrder = StandingOrders.None;
            }
        }
        loadedUnits.RemoveRange(0, loadedUnits.Count); //clear the list

    }


    public override bool IsFull => loadedUnits.Count() >= capacity;

    public override void UnloadUnit(IUnit u)
    {
        loadedUnits.Remove(u);
    }


    public override void Unload()
    {
        IUnit? volunteer = null;
        foreach (var a in loadedUnits)
        {
            a.StandingOrder = StandingOrders.None;
            volunteer = a;
        }
        //if( volunteer != null) volunteer.
        //forgot the idea here
        //the idea was to force wait on the container ship, and put the volunteer on the top 
        //also, only if land (or a city) is available for the army
    }

    public override void Load()
    {
        //look around the ship, set their standing order to LongMoveTo my location

        this.standingOrder = StandingOrders.Load;
        //standing order step


    }


    //pick up 
    public override void MoveTo(int x, int y)
    {
        //transport should pick up all the armies if it is leaving a city

        City? city = player.FindCity(this.X, this.Y);
        bool isCurrentlyInCity = city != null;

        if (isCurrentlyInCity)
        {
            //list all the armies in the city
            var armies = player.GetUnitsAtLoc(this.X, this.Y)
                .Where(a => a.GetType() == typeof(Army))
                .Select(a => a as Army);

            //all the armies in the city should be contained
            if (armies.Where(a => !a.IsContained).Any())
            {
                Debug.Assert(false, "armies in a city, but not contained");
            }

            //armies.ToList().ForEach(army => this.LoadUnit(army, this.X, this.Y));
            foreach (var ar in armies.ToList())
            {
                if (!IsFull)
                {
                    this.LoadUnit(ar, this.X, this.Y);
                }
            }


        }
        base.MoveTo(x, y);

    }




    public override int Capacity => 6; //change with dmg 
    public override int LoadedUnitsCount => loadedUnits.Count();


    public override int FullHitpoints() => 3;

    public override int FullSteps() => 2;
    public override int BaseFoggyType => (int)FoggyMap.transport;


    public override bool CanStepOn(MapType type) => type == MapType.sea;


    //public virtual bool CanAttackIt(int type)
    public override bool CanAttackIt(MapType type)
    {
        //if opponents city, no
        if ( type == MapType.city) return false; //only armies can attack cities

        //if own, can't
        //if opponents unit, perhaps
        return false;

    }
    //public override FoggyMap GetUnitType()
    //{ //this was used from the GUI rendering code
    //    return FoggyMap.transport;
    //}

    public override bool CanPickUp(IUnit? unit)
    {
        if (unit is null) return false;
        return unit.GetType() == typeof(Army);
    }



    public override string DisplayActivationMessage 
        => $"Transporter {Name} active, carrying {loadedUnits.Count()} armies.";



} //end class Transport



public class Fighter : Army
{
    public Fighter(int x, int y, Player p, int stepsLeft = -1, int hitPointsLeft = -1) : base(x, y, p, stepsLeft, hitPointsLeft)
    {
    }
}

public class Carrier : Transport
{


    public Carrier(int x, int y, Player p) : base(x, y, p)
    {
        name = "Carrier no " + Army.count;
    }

}



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
    Load,
}



