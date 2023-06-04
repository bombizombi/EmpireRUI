using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Empire.ViewModels;
using Empire.Models;

namespace Empire.Views

{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        public static RoutedCommand DebugCmd = new RoutedCommand();
        public static RoutedCommand ProductionDialogTestCmd = new RoutedCommand();
        public static RoutedCommand AsyncTestCmd = new RoutedCommand();
        public static RoutedCommand Window2Cmd = new RoutedCommand();

        public static RoutedCommand Sentry = new RoutedCommand();
        public static RoutedCommand Wait = new RoutedCommand();
        public static RoutedCommand Unload = new RoutedCommand();
        public static RoutedCommand Explore = new RoutedCommand();

        public static int CELL_W = 30;
        public static int CELL_H = 30;

        //for the map blocks, we well keep a reference inside a 2d array
        //for the army blocks, we need a dict since maps are scattered 
        public MainWindow2(MapViewModel mvm)
        {
            map = mvm;
            InitializeComponent();
            DataContext = map;

            copyOfMapBlocks = new MapBlock[map.map2sizeX, map.map2SizeY];
            copyOfArmyBlocks = new Dictionary<IUnit, MapBlock>();

            //extract
            CreateOrUpdateMapBlocks();
            CreateOrUpdateArmyBlocks(mvm);
            mvm.MapRefreshed += OnMapRefreshed;
            mvm.Feedback += OnFeedback;

            //also do a refresh on window startup


        }
        private MapViewModel map;

        private MapBlock[,] copyOfMapBlocks;
        private Dictionary<IUnit, MapBlock> copyOfArmyBlocks;
        //private MapBlock[,] copyOfArmyBlocks;


        private MapViewModel GetVM()
        {
            return (MapViewModel)this.DataContext;
        }

        private void CreateOrUpdateMapBlocks()
        {
            map.app.ActivePlayer.RenderFoggy();
            var fog = map.app.ActivePlayer.map;

            //create only needed blocks
            for (int y = 0; y < map.map2SizeY; y++)
            {
                for (int x = 0; x < map.map2sizeX; x++)
                {

                    if (copyOfMapBlocks[x, y] == null)
                    { 
                        CreateCellMap(map, (FoggyMap)fog[x, y].type, x, y);
                    }
                    else 
                    {
                        UpdateMapCell((FoggyMap)fog[x, y].type, x, y);
                    }
                }
            }

        }
        private void CreateOrUpdateMapBlock_forLocs(List<Loc> locations)
        {

            var fog = map.app.ActivePlayer.map;

            foreach (var loc in locations)
            {
                if (copyOfMapBlocks[loc.x, loc.y] == null)
                {
                    CreateCellMap(map, (FoggyMap)fog[loc.x, loc.y].type, loc.x, loc.y);
                }
                else
                {
                    UpdateMapCell((FoggyMap)fog[loc.x, loc.y].type, loc.x, loc.y);
                }

            }
        }


        private void UpdateMapCell(FoggyMap type, int x, int y)
        {
            MapBlock mb = copyOfMapBlocks[x, y];
            CellViewModel? cvm = mb.DataContext as CellViewModel;
            if (cvm != null)
            {
                if( x == 11 && y == 4)
                {
                   //Debugger.Break();
                }
                if (cvm.CellType == FoggyMap.army)
                {
                    Debugger.Break();
                }


                cvm.UpdateCellTypeIfNeeded(type);


                ChangeTemplate(cvm, mb);

                /*
                //copy in both Update and Create below
                ControlTemplate? template = (int)cvm.CellType switch
                {
                    _ when FoggyMapElem.IsCity(cvm.CellType) => (ControlTemplate)mb.Resources["tCity"],
                    >= (int)FoggyMap.army and < (int)FoggyMap.army + 10 => (ControlTemplate)mb.Resources["tArmy"],
                    _ => null
                };

                if (template is not null)
                {
                    mb.Template = template;
                }
                */
                /*
                if (FoggyMapElem.IsCity(cvm.CellType))
                {
                    
                    ControlTemplate template = (ControlTemplate)mb.Resources["tCity"];
                    mb.Template = template; ;
                } */



                /*
                if (cvm.CellType == FoggyMap.city)
                {
                    ControlTemplate template = (ControlTemplate)mb.Resources["tCity"];
                    mb.Template = template; ;

                }
                else
                {
                    //mb.Template = null;
                }
                */

                if( (int)cvm.CellType == 11)
                {
                    //Debugger.Break();
                }


            }
        }

        private void ChangeTemplate(CellViewModel cvm, MapBlock mb)
        {

            //copy in both Update and Create below

            ControlTemplate? template = (int)cvm.CellType switch
            {
                _ when FoggyMapElem.IsCity(cvm.CellType) => (ControlTemplate)mb.Resources["tCity"],
                >= (int)FoggyMap.army and < (int)FoggyMap.army + 10 => (ControlTemplate)mb.Resources["tArmy"],
                _ => null
            };

            if (template is not null)
            {
                mb.Template = template;
            }
        }


        private void CreateCellMap(MapViewModel map, FoggyMap type, int x, int y)
        {
            CellViewModel cvm = new CellViewModel(map, "e" + x + y, type, x, y);

            //extract
            MapBlock mb = new MapBlock();
            mb.DataContext = cvm;

            //copy in both Update and Create below

            /*
            if ( cvm.CellType == FoggyMap.cityNeutral)
            {
                ControlTemplate template = (ControlTemplate)mb.Resources["tCity"];
                mb.Template = template; ;

            } else
            {
                //mb.Template = null;
            }
            */

            ChangeTemplate(cvm, mb);

            /*
            ControlTemplate? template = (int)cvm.CellType switch
            {
                _ when FoggyMapElem.IsCity(cvm.CellType) => (ControlTemplate)mb.Resources["tCity"],
                >= (int)FoggyMap.army and < (int)FoggyMap.army + 10 => (ControlTemplate)mb.Resources["tArmy"],
                _ => null
            };

            if (template is not null)
            {
                mb.Template = template;
            }
            */



            Canvas.SetLeft(mb, x* CELL_W);
            Canvas.SetTop(mb, y* CELL_H);
            mapCanvas.Children.Add(mb);

            copyOfMapBlocks[x, y] = mb;
        }

        private MapBlock lastAnimatedBlock;
        private void StartActiveArmyAnimation(MapBlock mb)
        {
            if( lastAnimatedBlock != null)
            {
                lastAnimatedBlock.StopAnimationForActiveCell();
            }
            mb.StartAnimationForActiveCell();
            lastAnimatedBlock = mb;
        }

        private void StopActiveArmyAnimation(IUnit army)
        {
            var mb = copyOfArmyBlocks[army]; 
            if (mb != null)
            {
                mb.StopAnimationForActiveCell();
            }
        }


        private async Task OnFeedback(MapViewModel mvm)
        {
            //process feedback animations

            FeedbackTasks.Task? task = mvm.GetFeedbackTask();
            while (task != null)
            {
                await OnFeedback_ForTask(task);
                task = mvm.GetFeedbackTask();
            }
        }

        private async Task OnFeedback_ForTask(FeedbackTasks.Task task)
        {
            //switch task

            IUnit army = task.unit;

            var mb = copyOfArmyBlocks[army]; //?
            if (task.locs != null)
            {
                CreateOrUpdateMapBlock_forLocs(task.locs);
            }
            EnsureVisible(army, mb, 0);
            UpdateForArmy(army, mb);

            switch (task.kind)
            {
                case Tasks.DelayAfterMove:
                    await mb.GetStoryboard("animDelayAfterMove")?.BeginAsync();
                    break;
                case Tasks.DelayBeforeMove:
                    await mb.GetStoryboard("animDelayBeforeMove")?.BeginAsync();
                    break;
                case Tasks.DelayInbetweenSteps:
                    await mb.GetStoryboard("animDelayInbetweenMove")?.BeginAsync();
                    break;
                case Tasks.StopBeingActive:
                    StopActiveArmyAnimation(task.unit);
                    break;
            }

        }





        private void CreateOrUpdateArmyBlocks(MapViewModel mvm)
        {
            var armies = map.app.ActivePlayer.GetArmies();

            //var active = map.app.ActivePlayer.ActivateUnit();
            //changed to ActiveUnit so wait command will work
            var active = map.app.ActivePlayer.ActiveUnit;

            foreach (var b in copyOfArmyBlocks) b.Value.MarkAsDead();

            foreach (var army in armies)
            {
                bool notActiveArmy = army != active;
                if (!army.IsVisible() && notActiveArmy) continue;

                MapBlock mb;
                if(!copyOfArmyBlocks.ContainsKey(army))
                {
                    //create;

                    //CellViewModel cvm = map.map2[x, y];
                    //army.GetUnitType() assumes all the armies are mine (puts the base type number for display
                    CellViewModel cvm = new CellViewModel(map, "e" + army.Name, army.GetUnitType(), army.X, army.Y);

                    //extract
                    mb = new MapBlock();
                    mb.DataContext = cvm;

                    Canvas.SetLeft(mb, army.X * CELL_W);
                    Canvas.SetTop(mb, army.Y * CELL_H);
                    mapCanvas.Children.Add(mb);

                    copyOfArmyBlocks[army] = mb;

                    ChangeTemplate(cvm, mb);


                    /*
                    //duplicate
                    if (army == active)
                    {
                        EnsureVisible(army, mb, 1);
                        StartActiveArmyAnimation(mb);
                    }*/


                }
                else
                {
                    //update
                    mb = copyOfArmyBlocks[army];
                    UpdateForArmy(army, mb);
                    ChangeTemplate(mb.DataContext as CellViewModel, mb);

                    /*
                    //duplicate
                    if( army == active)
                    {
                        EnsureVisible(army, mb, 1);
                        StartActiveArmyAnimation(mb);
                    }*/

                }

                if (army == active)
                {
                    EnsureVisible(army, mb, 1);
                    StartActiveArmyAnimation(mb);
                }




                mb.MarkAsAlive();

            }

            /* remove unused blocks */
            var armiesToDelete = from a in copyOfArmyBlocks
                                 where a.Value.dead
                                 select a;
            foreach( var bl in armiesToDelete) 
            {
                mapCanvas.Children.Remove(bl.Value);
                copyOfArmyBlocks.Remove(bl.Key); 
            }


        }

        private void UpdateForArmy(IUnit army, MapBlock mb)
        {
            //do i need to update anyting else?
            /* moving the control will happen on render, do not read the new coordinates here.
             * Read the old coordinates, calc the difference
             */
            Point p = mb.TranslatePoint(new Point(0, 0), mapCanvas); //read the current position 
            double newX = army.X * CELL_W;
            double newY = army.Y * CELL_H;

            Canvas.SetLeft(mb, newX);
            Canvas.SetTop(mb, newY);

            //Debug.WriteLine($"army: {army.Name} {army.X}");
            //mvm.SetStatus($"test. set to {newX} {newY} read {p} ");



            var cvm = (mb.DataContext as CellViewModel);
            //cvm.x = army.X;
            //cvm.y = army.Y;
            cvm.UpdateXY(army.X, army.Y, CELL_W);


        }


        private void EnsureVisible(IUnit army, MapBlock mb, int moatSize)
        {


            double newX = army.X * CELL_W;
            double newY = army.Y * CELL_H;
            Point p = mb.TranslatePoint(new Point(0, 0), mapCanvas); //read the current position 
            double deltaX = newX - p.X;
            double deltaY = newY - p.Y;



            ScrollViewer? sv = mapCanvas.Parent as ScrollViewer;
            //see if active army is visible

            //Rect viewport = new Rect(new Point(0, 0), sv.RenderSize);
            Rect viewport = new Rect(new Point(0, 0), new Point(sv.ViewportWidth, sv.ViewportHeight));

            GeneralTransform transform = mb.TransformToAncestor(sv);
            //known to crash here when a breakpoint was set on the CellViewModel image name call

            //Rect egzBeforeRendedint = transform.TransformBounds(new Rect(new Point(0, 0), mb.RenderSize));
            //calculate the current positon from deltas
            Rect egz = transform.TransformBounds(new Rect(new Point(deltaX, deltaY), mb.RenderSize));

            Rect childBounds = new Rect(
                new Point(egz.Left - moatSize * CELL_W, egz.Top - moatSize * CELL_H),
                new Point(egz.Right + moatSize * CELL_W, egz.Bottom + moatSize * CELL_H));
            bool isVisible = viewport.IntersectsWith(childBounds);
            bool isContained = (viewport.Contains(childBounds.TopLeft)) && viewport.Contains(childBounds.BottomRight);

            //double dbgD = viewport.Bottom - childBounds.Bottom;
            //mvm.SetStatus($"test. distance from bottom{dbgD} ");

            if (!isContained)
            {
                double dx = (childBounds.Left + (childBounds.Width / 2)) - (viewport.Left + viewport.Width / 2);
                double scrollToX = Math.Max(0, sv.HorizontalOffset + dx);
                sv.ScrollToHorizontalOffset(scrollToX);

                double dy = (childBounds.Top + (childBounds.Height / 2)) - (viewport.Top + viewport.Height / 2);
                double scrollToY = Math.Max(0, sv.VerticalOffset + dy);
                sv.ScrollToVerticalOffset(scrollToY);
            }

        }




        public void OnMapRefreshed(MapViewModel sender)
        {
            //extract
            //mapCanvas.Children.Clear();
            CreateOrUpdateMapBlocks();
            CreateOrUpdateArmyBlocks( sender);

        }




        private void DebugCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DebugCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();
            vm.Debug_SmallerMap();
        }
        private void ProductionDialogTestCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ProductionDialogTestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();

            //fake show and hide production dialog


            var prodVM = new ProductionViewModel(vm);

            dialogProduction.DataContext = prodVM;
            dialogProduction.Visibility = Visibility.Visible;
            /* wtf
            dialogProduction.DataContext = prodVM;
            dialogProduction.Visibility = Visibility.Visible;
            */
            //? TODO

        }

        private void AsyncTestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var vm = GetVM();
            vm.TestAsync();



        }

        private void Window2Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var win2 = new MainWindow2(GetVM());

            win2.Show();

        }

        private void Cmd_Sentry(object sender, ExecutedRoutedEventArgs e)
        {
            GetVM().Sentry();
        }
        private void Cmd_Wait(object sender, ExecutedRoutedEventArgs e)
        {
            GetVM().Wait();
            //popupProduction.Visibility = Visibility.Visible;
        }
        private void Cmd_Unload(object sender, ExecutedRoutedEventArgs e)
        {
            GetVM().Unload();
        }

        private void Cmd_Explore(object sender, ExecutedRoutedEventArgs e)
        {
            GetVM().Explore();
        }



    }//end class MainWIndow2


}
