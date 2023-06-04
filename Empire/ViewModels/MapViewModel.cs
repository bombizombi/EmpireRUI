using Empire.Models;
using Empire.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Empire.ViewModels
{

    public delegate void MapRefreshedEventHandler(MapViewModel sender);
    public delegate Task FeedbackEventHandler(MapViewModel sender);


    public class MapViewModel : INotifyPropertyChanged, IFeedbacker

    {
        public EmpireTheGame app;



        //public List<string> EmpireMap
        public ObservableCollection<CellViewModel> EmpireMap
        {
            get;
            set;
        }





        public int MapColumns { get; set; }

        private DispatcherTimer timer;
        private int timerCount;


        public CellViewModel[,] map2;
        public int map2sizeX;
        public int map2SizeY;


        public MapViewModel()
        {
            SetupTimer();

            //hold the 
            SetStatus("Loading the map.");

            //debug world

            MapLoader map = new();

            //?
            MapColumns = 100;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MapColumns)));

            //EmpireMap = new List<string>();
            EmpireMap = new ObservableCollection<CellViewModel>();

            int mapx = 0;
            int mapy = 0;
            var increase = () =>
            {
                mapx++;
                if (mapx >= 100)
                {
                    mapx = 0;
                    mapy++;
                }
            };
            for (int i = 0; i < map.NumberOfCells; i++)
            {
                FoggyMap mapType = FoggyMapElem.ConvertFromTerrain(map.map[i]);
                CellViewModel cvm = new CellViewModel(this, "e" + i, mapType, mapx, mapy);
                EmpireMap.Add(cvm);
                increase();
                //EmpireMap.
            }

            //test map2 for Window2?
            CreateMap2(map);



            //army?
            app = new EmpireTheGame(map);
            //app.map = map;
            var armies = app.GetArmies();

            foreach (var army in armies)
            {
                int x = army.X;
                int y = army.Y;

                int mapPos = y * MapColumns + x;
                EmpireMap[mapPos].SetCellType(army);

            }





            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmpireMap)));

            //Init();


            m_tasks = new FeedbackTasks( this);


        }

        private void CreateMap2(MapLoader map)
        {
            map2 = new CellViewModel[map.sizeX, map.sizeY];

            int count = 0;
            for (int y = 0; y < map.sizeY; y++)
            {
                for (int x = 0; x < map.sizeX; x++)
                {
                    FoggyMap type = (FoggyMap)map.map[count];
                    CellViewModel cvm = new CellViewModel(this, "e" + count, type, x, y);
                    map2[x, y] = cvm;

                    count++;

                }
            }
            map2sizeX = map.sizeX;
            map2SizeY = map.sizeY;

        }

        private void RenderMap2()
        {

        }



        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            //Debug.Assert(false);

            timerCount++;
            //refresh map

            EmpireMap[0].DebugAnimate();

            RenderMap();
            RaiseRefreshEvent();

            StringBuilder rez = new StringBuilder();
            app.ActivePlayer.DebugStatus(rez);

            SetStatus($"tick {timerCount} {rez.ToString()}");
        }
        public void Init()
        {

            RenderMap();
            RaiseRefreshEvent();

            StringBuilder rez = new StringBuilder();
            app.ActivePlayer.DebugStatus(rez);

            SetStatus( $"tick {timerCount} {rez.ToString()}");
        }

        public event MapRefreshedEventHandler MapRefreshed;
        //public event PropertyChangedEventHandler? PropertyChanged;

        private void RaiseRefreshEvent()
        {
            if (MapRefreshed is not null)
            {
                MapRefreshed(this);
            }
        }


        FeedbackTasks m_tasks;
        public event FeedbackEventHandler Feedback;

        public async Task GiveFeedback()
        {
            if (Feedback != null)
            {
                await Feedback(this);
            }
        }

        public FeedbackTasks.Task? GetFeedbackTask()
        {
            if(! m_tasks.IsEmpty())
            {
                return m_tasks.Dequeue();
            }
            return null;
        }


        internal async Task TestAsync()
        {
            timer.IsEnabled = false;
            foreach (var army in app.ActivePlayer.GetArmies())
            {

                //EmpireMap[0].DebugAnimate();
                Test_RenderSingleArmy( army);
                await Task.Delay(500);

            }
            timer.IsEnabled = true;

        }

        public event PropertyChangedEventHandler? PropertyChanged;




        public void DummyDel()
        {
            EmpireMap.RemoveAt(0);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmpireMap)));
        }
        public void DummyAdd()
        {
            EmpireMap.Add( new CellViewModel( this, "new", 0, -1, -1));
            string deb = nameof(EmpireMap);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmpireMap)));
        }

        public void Debug_SmallerMap()
        {

            var oldMap = EmpireMap;

            ObservableCollection<CellViewModel> map = new();

            //old map is 100x 60?
            //target 20 20

            int x = 0; int y = 0;
            for (int i = 0; i < 100 * 60; i++)
            {
                if( (x < 15) && (y < 15))
                {
                    var cell = oldMap[i];
                    map.Add(cell);
                }
                x++;
                if(x >= 100)
                {
                    x = 0; 
                    y++;
                }

            }

            EmpireMap = map;

            //uniGrid.
            MapColumns = 15;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MapColumns))); //wtf

            SetStatus("Smaller world.");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmpireMap)));

        }


        public void RenderMap()
        {


            app.ActivePlayer.RenderFoggy();

            var fog = app.ActivePlayer.map;

            //from the data in the model, create cell map
            ObservableCollection<CellViewModel> map = new();

            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    CellViewModel cvm = new CellViewModel(this, "e" + x+","+y, (FoggyMap)fog[x,y].type, x, y);
                    map.Add(cvm);
                }
            }


            var armies = app.ActivePlayer.GetArmies();
            //var active = app.ActivePlayer.GetActiveArmy();
            var active = app.ActivePlayer.ActiveUnit;
            if (active == null)
            {
                //active = app.ActivePlayer.ActivateUnit(m_tasks);



                //might still be null
            }

            //activateUnit here is not guaranteed to actually activate something, if all units are on standing orders
            //?

            foreach (var army in armies)
            {
                int mapPos = army.Y * 15 + army.X;

                //actve army blinks
                if (army == active)
                {
                    if (timerCount % 2 == 0)
                    {
                        if( mapPos < map.Count)
                            map[mapPos].SetCellType(army);
                    } //otherwise, just show terain
                }
                else
                {
                    if (mapPos < map.Count)
                        map[mapPos].SetCellType(army);
                }

            }

            EmpireMap = map;


            //uniGrid.
            MapColumns = 15;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MapColumns))); //wtf

            //SetStatus("Smaller world with fog.");

            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmpireMap)));
        }



        public string StatusText { get; set; }
        public void SetStatus(string s)
        {
            StatusText = s;
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
        }



        public void CellClicked(CellViewModel cell)
        {
            cell.Clicked();
        }




        internal async void MapClicked(int x, int y)
        {
            //any gui command should have the same pre and post processing



            timer.Stop();


            //move army

            var s = app.DebugDump();


            //FeedbackTasks tasks = new FeedbackTasks();
            Debug.Assert(m_tasks.IsEmpty(), "looks like tasks were not cleared before doing the next move");

            await app.MoveTo(x, y, m_tasks);
            await GiveFeedback();


            //give visual feedback 

            CheckFreshlyConqueredCities();


            await app.CheckEndOfTurn(m_tasks);
            await GiveFeedback();
            //extract loop from here


            RenderMap(); //this no good, render foggy map should be done after every step, and it all happens in the MoveTo

            RaiseRefreshEvent();

            UpdateAllCells();

        }




        internal async void MapLongClicked(int x, int y)
        {
            var s = app.DebugDump();

            await app.LongMoveTo(x, y, m_tasks);
            await GiveFeedback();

            //TODO should do the same things as in after app.MoveTo
            RenderMap(); //this no good, render foggy map should be done after every step, and it all happens in the LongMoveTo
            RaiseRefreshEvent();

            UpdateAllCells();

        }

        private void UpdateAllCells()
        {
            //update fire sale, just to avoid raising events on the (?)
            foreach (var cell in EmpireMap)
            {
                cell.UpdateOnModelChange();
            }

        }


        private ObservableCollection<CellViewModel> RenderVisibleMapSection(FoggyMapElem[,] fog)
        {
            //from the data in the model, create cell map
            ObservableCollection<CellViewModel> map = new();

            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    CellViewModel cvm = new CellViewModel(this, "e" + x + "," + y, (FoggyMap)fog[x, y].type, x, y);
                    map.Add(cvm);
                }
            }
            return map;

        }


        public void Test_RenderSingleArmy( IUnit armyToDisplay)
        {
            //app.ActivePlayer().RenderFoggy();

            var fog = app.ActivePlayer.map;

            var map = RenderVisibleMapSection(fog);

            int mapPos = armyToDisplay.Y * 15 + armyToDisplay.X;
            map[mapPos].SetCellType(armyToDisplay);

            EmpireMap = map;


            SetStatus("This is the army " + armyToDisplay.Name);

        }

        private void CheckFreshlyConqueredCities()
        {
            City c = app.ActivePlayer.FindFreshlyConqueredCity();
            if (c == null) return;

            //pop up time
            //popup
            var pvm = new ProductionViewModel(this);
            ProductionModalWindow win = new ProductionModalWindow(pvm);
            win.ShowDialog();
            var rez = pvm.UnitsArray;
            c.SetProduction(pvm.SelectedUnit);


        }


        public async Task Sentry()
        {
            await app.Sentry(m_tasks);
            await GiveFeedback();

            //end of move check?
            await app.CheckEndOfTurn(m_tasks);
            await GiveFeedback();

        }
        public async Task Wait()
        {
            app.Wait(m_tasks);
            await GiveFeedback();
            //end of move check?
        }
        public async void Unload()
        {

            app.Unload(m_tasks);
            await GiveFeedback();
        }

        public async Task Explore()
        {

            //any gui command should have the same pre and post processing



            //await app.LongMoveTo(x, y, m_tasks);
            //wait GiveFeedback();

            app.Explore(m_tasks);
            await GiveFeedback();

            //TODO should do the same things as in after app.MoveTo
            RenderMap(); //this no good, render foggy map should be done after every step, and it all happens in the LongMoveTo
            RaiseRefreshEvent();

            UpdateAllCells();

            await app.CheckEndOfTurn(m_tasks);
            await GiveFeedback();


            //old mystery calls
            RenderMap(); //this no good, render foggy map should be done after every step, and it all happens in the MoveTo
            RaiseRefreshEvent();
            UpdateAllCells();

            //22222222222


            //end of move check?
        }



        public async Task Command_EndOfTurn()
        {
            app.Command_EndOfTurn(m_tasks);
            await GiveFeedback();
        }


    }






}
