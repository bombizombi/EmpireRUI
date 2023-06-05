using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.Models
{
    public class EmpireTheGame
    {
        public MapLoader map;
        public Player[] players;
        //private Map map;

        public EmpireTheGame(MapLoader m)
        {
            map = m;


            players = new Player[2]; ;
            players[0] = new Player(this);

            var ar = new Army(5, 5, players[0]);
            players[0].AddUnit(ar);
            var ar2 = new Army(4, 4, players[0]);
            players[0].AddUnit(ar2);

            //var p1 = new Fighter(66, 66, players[0]);
            var p1 = new Fighter(3, 13, players[0]);
            p1.standingOrder = StandingOrders.LongGoto;
            p1.TargetX = 8;
            p1.TargetY = 13;
            players[0].AddUnit(p1);

            //players[0].ActivateUnit();

            //players[0].AddCity(11, 4);

            //create an opponent and a fake city on the map
            players[1] = new Player(this);
            map.DebugFakeCity();
            players[1].AddCity(2, 4); //connected to the DebugFakeCity


        }

        public List<IUnit> GetArmies()
        {
            //temp single player only
            var player = players[0];
            return player.GetArmies();
        }

        public Player ActivePlayer
        {
            get{ return players[0]; }
        }

        public string DebugDump()
        {
            //dump all armies
            StringBuilder rez = new();
            rez.AppendLine($"active: ");
            foreach (var a in players[0].GetArmies())
            {
                rez.AppendLine($"Army {a.StepsAvailable} on {a.X},{a.Y}");
            }
            return rez.ToString();
        }


        private bool CheckMapEdges(int x, int y)
        {
            //can only move to land close by, and can't step on the edge
            if (x == 0) return false;
            if (y == 0) return false;
            if (x == map.sizeX - 1) return false;
            if (y == map.sizeY - 1) return false;
            return true;
        }

        //one of the game commands?

        public async Task<bool> MoveTo(int x, int y, FeedbackTasks tasks)
        {
            //normal move means only neighbour cells, even for units with longer range

            //can only move to land close by, and can't step on the edge
            if (!CheckMapEdges(x,y)) return false;

            var army = ActivePlayer.ActiveUnit;

            //having no active army should be a normal state
            //add potential other action for clicks here
            if( army == null) return false;
            //Debug.Assert( army != null , "trying to move, but no unit is active");   


            if( Math.Abs( y - army.Y ) > 1) return false;
            if( Math.Abs( x - army.X ) > 1) return false;

            bool doNotAllowGoingNowhere = (army.X == x) && (army.Y == y);
            if (doNotAllowGoingNowhere ) return false;




            //get land type
            MapType type = map.map[x + y * map.sizeX];

            //this is no good.  Armies attack enemy and neutral cities, but enter their cities

            if (type == MapType.city) return Move_HandleCity(army, x, y);



            //type here has not enough information
            /*
                        //check unit if it hates it
                        if(army.CanAttackIt(type))
                        {
                            //well, attack it is

                            //attack on a city?

                            //army.Attack( ?);
                            army.AttackCity();

                            //fight visaul feedback

                            //? yeah, but what about enemy units
                            Debugger.Break();

                        }*/

            //check if it can hop on it
            var ship = ActivePlayer.FriendlyContainerAtLoc(x, y);
            if (ship != null) return Move_HandleLoading(army, x, y, ship);


            //check if it can bombard it


            //check unit if it likes it
            if (!army.CanStepOn(type)) return false;

            if (army.StepsAvailable == 0)
            {
                Debug.Assert(false, "why was unit with 0 steps active?");
                return false;
            }


            await army.MoveTo(x, y, tasks);



            //bool isRiding = false; supress warning
            foreach( var sh in ActivePlayer.GetArmies())
            {
                if (sh.ShipContainsUnit(army))
                {
                    sh.UnloadUnit(army);
                    army.Unload();
                    break;
                }
            }

            return true;
        }

        private bool Move_HandleCity(IUnit u, int x, int y)
        {
            City? city = ActivePlayer.FindCity(x, y);
            bool cityOwnedByPlayer = city != null;
            if (cityOwnedByPlayer)
            {
                u.X = x;
                u.Y = y;
                u.StepsAvailable -= 1;

                //enter the city
                u.EnterCity();

                return true;
            }
           

            //check unit if it hates it
            if (u.CanAttackIt(MapType.city))
            {
                //well, attack it is

                //attack on a city?

                bool cityAttackWon = u.AttackCity();
                if(cityAttackWon)
                {
                    //remove from previous owner, also handle units inside TODO
                    ActivePlayer.AddCity(x, y);
                }
                else
                {
                }
                u.Die(); //winning army spreads around the city and lives a long peaceful life

                //fight visaul feedback?

                //? yeah, but what about enemy units
                //Debugger.Break();

            }
            return true;

        }

        private bool Move_HandleLoading(IUnit u, int x, int y, IUnit ship)
        {
            //only ships to transports and planes to carriers
            bool caseArmyToShip = (u.GetType() == typeof(Army)) && (ship.GetType() == typeof(Transport));
            bool casePlaneToCarrier = (u.GetType() == typeof(Fighter)) && (ship.GetType() == typeof(Carrier));

            if (!(caseArmyToShip || casePlaneToCarrier)) return false;

            ship.LoadUnit(u, x, y);
            return true;

        }



        public async Task<bool> LongMoveTo(int x, int y, FeedbackTasks tasks)
        {
            


            //long move:
            //           -save target cell
            //           -find path (major pita)
            //           -do moves up to unit steps left

            if (!CheckMapEdges(x,y)) return false; ;

            var army = ActivePlayer.ActiveUnit;

            if (army == null) return false;

            //long move
            //if (Math.Abs(y - army.y) > 1) return false;
            //if (Math.Abs(x - army.x) > 1) return false;
            if ((army.X == x) && (army.Y == y)) return false;
            //also do not alow going nowhere



            //validate?

            //get land type
            MapType type = map.map[x + y * map.sizeX];

            //might be able to send it into unknown land type
            //for now, leave checks the same as for a single step

            //check unit if it likes it
            if (!army.CanStepOn(type)) return false;

            //checks passed, save target cell
            army.StandingOrder = StandingOrders.LongGoto;
            army.TargetX = x;
            army.TargetY = y;

            //any actual move will be done in the next turn as well.

            //terrain check should be moved to the steps part from the init part

            //extract actual long step
            await LongMoveStep(army, tasks);


            await CheckEndOfTurn(tasks);

            //end extract actual long step

            return true;
        }




        public async Task<bool> LongMoveStep(IUnit army, FeedbackTasks tasks)
        {

            await tasks.Add(army, Tasks.DelayBeforeMove);

            do
            {

                int deltax = army.TargetX - army.X;
                int deltay = army.TargetY - army.Y;
                //TODO this dissregards terrain or any armies in the way

                Debug.Assert(!((deltax == 0) && (deltay == 0)), "long goto with no deltas");

                //trivial path finding
                int stepX = 0, stepY = 0;
                if (deltax != 0) stepX = deltax / Math.Abs(deltax);
                if (deltay != 0) stepY = deltay / Math.Abs(deltay);


                if (army.StepsAvailable == 0)
                {
                    await tasks.Add(army, Tasks.DelayAfterMove);
                    return false;
                }


                army.X += stepX; //this is wrong
                army.Y += stepY;
                army.StepsAvailable -= 1;
                var locs = army.RenderFoggy();
                await tasks.Add(army, Tasks.DelayInbetweenSteps, locs);

                //target reached?
                if ((army.X == army.TargetX) && (army.Y == army.TargetY))
                {
                    army.StandingOrder = StandingOrders.None;
                    //clear target coordinates
                    army.TargetX = -1;
                    army.TargetY = -1;
                    await tasks.Add(army, Tasks.DelayAfterMove);
                    break;
                }

            } while (army.StepsAvailable > 0);

            return army.StepsAvailable == 0;

        }

        public async Task Sentry(FeedbackTasks tasks)
        {
            //TODO feedback on sentry
            var army = ActivePlayer.ActiveUnit;
            if (army == null) return;
            tasks.Add(army, Tasks.StopBeingActive);
            army.StandingOrder = StandingOrders.Sentry;

        }

        public async Task Wait(FeedbackTasks tasks)
        {
            var armyToWait = ActivePlayer.ActiveUnit;

            //try to activate somebody else
            IUnit next = await ActivePlayer.ActivateUnit(tasks, armyToWait);

            if( next == null)
            {
                ActivePlayer.ActiveUnit = armyToWait; //looks like this is the onl one left to move
            }

        }

        public void Unload(FeedbackTasks tasks)
        {
            var army = ActivePlayer.ActiveUnit;
            if (army == null) return;
            army.Unload();

            Wait(tasks);
        }


        public async Task<bool> Explore(FeedbackTasks tasks)
        {
            //var army = ActivePlayer.ActiveUnit;
            //if (army == null) return;

            //army.Explore();



            //xx;

            //explore:
            //           -set standing orders
            //           -do explore moves up to unit steps left


            var army = ActivePlayer.ActiveUnit;
            if (army is null) return false;


            //trivial stupid explore, just step in the direction of a first unexplored tile
            //should clear standing orders is everything around it is explored or if an enemy army or city is detected
            

            //find first legit neighbour tile

            bool nothingToExplore = false;

            if (nothingToExplore)
            {
                army.StandingOrder = StandingOrders.None;
            } else
            {
                //go up
                int x = army.X + 1;
                int y = army.Y - 1;
                MapType type = map.map[x + y * map.sizeX];


                //check unit if it likes it
                if (!army.CanStepOn(type)) return false;
                //await LongMoveStep(army, tasks);

                //baci up to here
                army.StandingOrder = StandingOrders.Explore;


                await ExploreStep(army, tasks);


            }

            //if we found a possible step

            //my current pos?
            //Debugger.Break();
            /*
            xx;




            //get land type
            MapType type = map.map[x + y * map.sizeX];

            //might be able to send it into unknown land type
            //for now, leave checks the same as for a single step

            //check unit if it likes it
            if (!army.CanStepOn(type)) return false;



            //check unit if it likes it
            if (!army.CanStepOn(type)) return false;

            //checks passed, save target cell
            army.StandingOrder = StandingOrders.Explore;
            army.TargetX = -1;
            army.TargetY = -1;

            //any actual move will be done in the next turn as well.

            //terrain check should be moved to the steps part from the init part


            //extract actual long step
            await ExploreStep(army, tasks);


            await CheckEndOfTurn(tasks);

            //end extract actual long step
            */

            Wait(tasks);


            return true;
        }

        //222222222222222222222222222222222222222222222222222222222222


        //11111111

        public async Task<bool> ExploreStep(IUnit army, FeedbackTasks tasks)
        {
            Debug.WriteLine("Before tasks add");
            // await tasks.Add(army, Tasks.DelayBeforeMove);
            Debug.WriteLine("After tasks add");

            do
            {
                Debug.WriteLine("do loop");

                //find the maximum fog reveal step
                bool thereIsMoreToExplore = FindMaximumForRevealStep(army,  out int stepX, out int stepY);

                if( !thereIsMoreToExplore)
                {
                    army.StandingOrder = StandingOrders.None;
                    await tasks.Add(army, Tasks.DelayAfterMove);
                    break;
                }

                if (army.StepsAvailable == 0)
                {
                    await tasks.Add(army, Tasks.DelayAfterMove);
                    return false;
                }

                //TODO moving army without any checks
                //should share code with normal Move checks

                army.X += stepX; //this is wrong
                army.Y += stepY;




                army.StepsAvailable -= 1;
                var locs = army.RenderFoggy();
                await tasks.Add(army, Tasks.DelayInbetweenSteps, locs);

                /*
                //target reached?
                if ((army.X == army.TargetX) && (army.Y == army.TargetY))
                {
                    army.StandingOrder = StandingOrders.None;
                    //clear target coordinates
                    army.TargetX = -1;
                    army.TargetY = -1;
                    await tasks.Add(army, Tasks.DelayAfterMove);
                    break;
                }
                */
            } while (army.StepsAvailable > 0);

            return army.StepsAvailable == 0;

        }
        //2222222222

        private bool FindMaximumForRevealStep(IUnit army, out int stepX, out int stepY)
            //return true if there is something to explroe
        //TODO actual method
        {
            int locX = army.X;
            int locY = army.Y;

            //switch to circular array of 8 possible directions
            for (int dy = -1; dy <= 1; dy++)
            {
                for( int dx = -1; dx <= 1; dx++)
                {
                    if (dx == dy) continue; //skip 0,0 ????
                    /*
                    MapType type = map.map[ (locX+dx)  + (locY + dy) * map.sizeX];
                    if (!army.CanStepOn(type)) return false;
                    */

                }
            }
            //go up 
            stepX = 1;
            stepY = -1;

            //temp boundary check
            int rezX = army.X + stepX;
            int rezY = army.Y + stepY;

            if (rezX <= 0) return false;
            if( rezY <= 0) return false;
            if (rezX >= map.sizeX - 1) return false;
            if (rezY >= map.sizeY - 1) return false;

            return true;
        }




        public async Task CheckEndOfTurn(FeedbackTasks tasks)
            //
        {
            //try to activate something else, if not, go to next step
            var next = await ActivePlayer.ActivateUnit(tasks);
            if( next == null)
            {
                //end of move processing

                //next player

                //hack, just reactivate
                await ActivePlayer.NewMove();
                await CheckEndOfTurn(tasks);
                //TODO fix single player (and check end of game?)

            }

        }

        public async void Command_EndOfTurn(FeedbackTasks tasks)
        {
            //forces end of turn (even if there are armies yet to move?)

            ActivePlayer.NewMove();

            //does nothing if no army needs a move
            //var next = ActivePlayer().ActivateUnit();
            ActivePlayer.ActivateUnit(tasks);


        }

    }

}
