using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Models
{

    public interface IUnit
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public int StepsAvailable { get; set; }
        public int Hitpoints { get; set; }

        public Task MoveTo(int x, int y, FeedbackTasks t);

        public void NewTurn();
        public StandingOrders StandingOrder { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }



        public bool IsVisible();
        public bool CanAttackIt(MapType type);
        public bool CanStepOn(MapType type);
        public bool IsInSentry();

        public void EnterCity();
        public bool AttackCity();
        public void LoadContainer(); //this unit enters a container
        public void LoadUnit(IUnit u, int x, int y); //this containers gets a new passanger unit
        public void UnloadUnit(IUnit u);
        public void Unload();
        //public void Explore();
        public bool ShipContainsUnit(IUnit u);
        public List<Loc> RenderFoggy();


        public FoggyMap GetUnitType();
        public void Die();


        public string DebugStatus();

    }

    public class Army : IUnit
    {
        //can walk on: 
        //   -land
        //can attack:
        //   -city
        //   -some ships?
        //   -anything on land

        private int x;
        private int y;
        public Player player;
        public string name;

        public int hitpoints;
        private int stepsAvailable = 1;

        public StandingOrders standingOrder;
        public int targetX, targetY;
        private bool unitIsContained;


        public int X { get { return x; } set { x = value; } }
        public int Y { get { return y; } set { y = value; } }
        public int Hitpoints { get { return hitpoints; } set { hitpoints = value; } }
        public int StepsAvailable {  get { return stepsAvailable; } set { stepsAvailable = value; } }
        public string Name { get { return name; } set { name = value; } }

        public StandingOrders StandingOrder { get { return standingOrder; } set { standingOrder = value; } }
        public int TargetX { get { return targetX; } set { targetX = value; } }
        public int TargetY { get { return targetY; } set { targetY = value; } }

        public virtual FoggyMap GetUnitType()
        {
            return FoggyMap.army; 
        }

        public Army(int x, int y, Player p)
        {
            Army.count++;

            this.x = x;
            this.y = y;
            hitpoints = FullHitpoints();
            stepsAvailable = FullSteps();


            player = p;
            name = "Army no " + Army.count;

        }

        public string DebugStatus()
        {
            //return($"{Name} {IsInSentry()} steps:{StepsAvailable}");
            return ($"{Name} {DebugSOrder()} steps:{StepsAvailable}");
        }
        public string DebugSOrder()
        {
            if (standingOrder == StandingOrders.None) return "";
            return $"[{standingOrder.ToString()}]";
        }



        protected static int count = 0;

        public virtual int FullHitpoints() => 1;
        public virtual int FullSteps() =>  1;
        public virtual int Capacity() => 0;


        public static Random rnd = new Random();

        public void NewTurn()
        {
            stepsAvailable = FullSteps();  //back to life
        }


        public async virtual Task MoveTo(int x, int y, FeedbackTasks tasks)
        {
            if(unitIsContained)
            {
                unitIsContained = false; //more unloading processing?
            }

            this.X = x;
            this.Y = y;
            Debug.Assert(StepsAvailable > 0, "no steps available");
            StepsAvailable -= 1;

            MoveContainedUnits(x,y);


            var locs = RenderFoggy();
            await tasks.Add(this, Tasks.StopBeingActive);
            await tasks.Add(this, Tasks.DelayInbetweenSteps, locs); 


        }

        public List<Loc> RenderFoggy()
        {
            //might return true if something of interest was discovered
            return this.player.RenderFoggyForArmy(this);
        }

        protected virtual void MoveContainedUnits(int x, int y)
        {
        }
        public virtual bool ShipContainsUnit(IUnit u) => false;
        public virtual void UnloadUnit(IUnit u)
        {
            Debug.Assert(false, "not a container");
        }


        public virtual void Unload() { }

        public bool IsVisible()
        {
            return !unitIsContained;
        }


        public virtual bool CanStepOn(MapType type) => type == MapType.land;
        public bool CanAttackIt(MapType type) => type == MapType.city;

        /* armies can attack:  armies, planes(if on land), cities
         * armies can bombard:  ships other than subs
         *
        public bool CanAttackIt(MapType type) => type is IDup;
        unit options as empty interfaces? */

        public bool IsInSentry()
        {
            return standingOrder == StandingOrders.Sentry;
        }


        public bool Attack( IUnit enemy) //returns true if won
        {
            do
            {
                //randomly substract hitpoints
                if (rnd.NextDouble() > 0.5)
                {
                    enemy.Hitpoints--;
                    if (enemy.Hitpoints == 0)
                    {
                        enemy.Die();
                        return true;
                    }
                }

                if (rnd.NextDouble() > 0.5)
                {
                    hitpoints--;
                    if (hitpoints == 0)
                    {
                        Die();
                        return false;
                    }
                }
            } while (true);
        }
        //attack trasporters?
        //bombard ships?
        public bool AttackCity()
        {
            //armies only?
            if (rnd.NextDouble() > 0.5)  //TODO configurable probs?
            {
                hitpoints--;
                if (hitpoints == 0)
                {
                    Die();
                    return false;
                }
            }
            return true;
        }

        public virtual void EnterCity()
        {
            //entering the city removes all steps, and resets unit range
            StepsAvailable = 0;
            //
            unitIsContained = true;
        }

        public virtual void LoadUnit(IUnit u, int x, int y) //this containers gets a new passanger unit
        {
            Debug.Assert(false, "");
        }


        public void LoadContainer()
        {
            standingOrder = StandingOrders.Sentry;
            unitIsContained = true;
        }


        public void Die()
        {
            player.UnitKilled(this);
        }

    } //end class Army





    public class Transport : Army
    {
        private int capacity;
        //private int loaded;

        private List<IUnit> loadedUnits;

        public Transport(int x, int y, Player p) : base(x, y, p)
        {

            name = "Transport no " + Army.count;

            capacity = Capacity();
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
            foreach( var u in loadedUnits)
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

            u.LoadContainer();

        }
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
        }

        public override int Capacity() => 6;
        public override int FullHitpoints()
        {
            return 3;
        }

        public override int FullSteps()
        {
            return 2;
        }


        public override bool CanStepOn(MapType type) => type == MapType.sea;

        public bool CanAttackIt(int type)
        {
            return type == 1; //TODO blah, enum needed
            //
        }
        public override FoggyMap GetUnitType()
        {
            return FoggyMap.transport;
        }



    } //end class Transport

    public class Fighter : Army
    {
        public Fighter(int x, int y, Player p) : base(x,y,p)
        {
            name = "Fighter no " + Army.count;
            ResetRange();
        }



        private int range;

        private void ResetRange()
        {
            range = 20;
        }


        public override int FullHitpoints()
        {
            return 1;
        }

        public override int FullSteps()
        {
            return 5;
        }


        public override bool CanStepOn(MapType type) => (type == MapType.sea) || (type == MapType.land);

        public bool CanAttackIt(int type)
        {
            return type == 1; //TODO blah, enum needed
            //
        }
        public override FoggyMap GetUnitType()
        {
            return FoggyMap.fighter;
        }

        public async override Task MoveTo(int x, int y, FeedbackTasks t)
        {
            await base.MoveTo(x, y, t);
            range -= 1;
            if (range == 0)
            {
                Die(); //no good if our last step was entering or loading
            }
            return;
        }



    }  //end class fighter


    public class Carrier : Transport
    {
        public Carrier(int x, int y, Player p) : base(x, y, p)
        {
            name = "Carrier no " + Army.count;
        }

    }

    public enum StandingOrders
    {
        None, 
        LongGoto,
        Sentry,
        Explore,

    }


    public enum ProductionEnum
    {
        army = 0,
        figher,
        destroyer,
        transport,
        submarine,
        cruiser,
        carrier,
        battleship,
        uninitialized

    }

    public class City
    {
        public int x;
        public int y;
        public ProductionEnum production;

        public int remaining;

        public City(int inx, int iny)
        {
            x = inx;
            y = iny;

            //production = ProductionEnum.army; //
            //production = ProductionEnum.transport; //
            production = ProductionEnum.uninitialized; //

            //remaining = freshProduction[(int)production];
            remaining = -1;
        }

        public void ResetProduction()
        {
            production = ProductionEnum.uninitialized; //
            remaining = -1;
        }


        public IUnit NewTurn(Player player)
        {

            remaining--;
            if( remaining == 0)
            {
                //create diff units from "production" var
                //Army a = new Army(x, y, player);
                IUnit a = CreateIUnit(production, player);
                player.AddUnit(a);

                //reset production
                remaining = continuingProduction[(int)production];

                return a;
            }
            return null;
        }

        public static int[] freshProduction =      new int[] { 6, 12, 24, 30, 24, 36, 48, 60 };
        public static int[] continuingProduction = new int[] { 5, 10, 20, 25, 20, 30, 40, 50 };


        //reuse city as an Unit factory
        public IUnit CreateIUnit(ProductionEnum prod, Player player)
        {
            IUnit? unit = null;
            switch (prod)
            {
                case ProductionEnum.army:
                    unit = new Army(x, y, player);
                    break;
                case ProductionEnum.transport:
                    unit = new Transport(x, y, player);
                    break;
                case ProductionEnum.figher:
                    unit = new Fighter(x, y, player);
                    break;

                default:
                    Debugger.Break();
                    break;

            }
            return unit;
        }

        public void SetProduction(int newProd)
        {
            if( newProd != (int)production)
            {
                production = (ProductionEnum)newProd;
                remaining = freshProduction[(int)production];

            }
        }



    }

    public class Map
    {

        //public 

    }

    public struct Loc
    {
        public int x;
        public int y;
    }

}
