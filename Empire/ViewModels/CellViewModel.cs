using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Diagnostics;
using Empire.Models;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace Empire.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler? PropertyChanged;

        private MapViewModel _mm;

        public int x;
        public int y;

        private int oldx;
        private int oldy;

        public string DebugName { get; set; }
        private FoggyMap type;

        private int width = 30;
        private int height = 30;

        private int CELL_SIZ;

        private DateTime mouseDownTime = DateTime.MinValue;  //set on mouse down, reset when? parent should probably record it

        public void UpdateXY(int inx, int iny, int wid)
        {
            //Debug.WriteLine($"******updating x {oldx} to {inx}");


            CELL_SIZ = wid;

            oldx = x;
            oldy = y;
            x = inx;
            y = iny;

            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(OldX));
            OnPropertyChanged(nameof(OldY));
        }

        //public MapType CellType
        public FoggyMap CellType
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CellType)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageName)));
                }
            }
        }

        public static Brush[] playerColors = new Brush[2]
        {
            Brushes.Yellow,
            Brushes.Red,
            //add players if needed
        };
        public Brush PlayerColor
        {
            get
            {
                if (type == FoggyMap.cityNeutral)
                {
                    return Brushes.White;
                } else
                {
                    if (type >= FoggyMap.city)
                    {
                        int index = (int)type % 10;
                        return playerColors[index];
                    }
                }
                return Brushes.Chocolate;
            }
        }

        public int X { 
            get 
            {
                //Debug.WriteLine($"reading x {CELL_SIZ * x}");
                return CELL_SIZ * x;

            } 
        }
        public int Y { get { return CELL_SIZ * y; }  }
        public int OldX { get { return CELL_SIZ * oldx; } }
        public int OldY { get { return CELL_SIZ * oldy; }  }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void UpdateOnModelChange()
        {
            OnPropertyChanged(nameof(ImageName));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageName)));
        }

        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }

        public string ImageName
        {
            get
            {

                switch (type)
                {
                    case FoggyMap.unknown:
                        return "images/unknown.bmp";
                    case FoggyMap.city:
                        return "images/city.bmp";
                        //return "";
                    case FoggyMap.sea:
                        return "images/sea.bmp";
                    case FoggyMap.land:
                        return "images/land.bmp";

                    case FoggyMap.army:
                        return "images/army1.bmp";
                    case FoggyMap.transport:
                        return "images/t1.bmp";
                    case FoggyMap.fighter:
                        return "images/f1.bmp";
                    default:
                        //Debug.Assert(false);
                        break;
                }
                return "images/sea.bmp";

            }
        }
        public CellViewModel(MapViewModel mm, string name, FoggyMap type, int x, int y)
        {
            _mm = mm;
            DebugName = name;
            CellType = type;
            this.x = x;
            this.y = y;

        }

        public void DebugAnimate()
        {
            int t = (int) type;
            t = t + 1;
            if (t > 3) t = 1;
            CellType = (FoggyMap)t;
        }


        //bug with overwriting armies, trigger only once
        private static bool bug1Hit = false;
        internal void SetCellType(IUnit army)
        {
            if (!bug1Hit)
            {
                Debug.Assert((int)CellType < 4, "overwritting army");  //ugly
            }
            if ((int)CellType >= 4)
            {
                bug1Hit = true;
            }


            //CellType = 4;
            CellType = army.GetUnitType();
        }
        internal void SetCellType(FoggyMap type)
        {
            //Debug.Assert(CellType < 4, "overwritting army");
            //CellType = 4;
            CellType = type;
        }


        public void UpdateCellTypeIfNeeded(FoggyMap newType)
        {
            if (CellType != newType)
            {
                SetCellType(newType);
            }
        }


        public void Clicked()
        {
            _mm.MapClicked(x, y);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageName)));

        }

        /* cell actions:
         * -short click: -should work in close cells only
         *               -far away cells should ignore it
         * -long click: -creates "GoTo" action, should work on all cells (as long as the target is correct for our unit
         * (attacks?)
         * 
         * -drag one cell into another: -can be quick
         *                              -need to save mouse-down-ed cell upstream?
         * 
         */

        public void OnMouseDown(MouseButtonEventArgs e)
        {
            mouseDownTime = DateTime.Now;
        }
        public void OnMouseUp(MouseButtonEventArgs e)
        {
            //this fcker only works because Render completely changes all the CellViewModels every 0.5 seconds

            if( mouseDownTime == DateTime.MinValue)
            {
                //TODO now what
                //Debugger.Break();
                //mouse down was in a differenct cell, do long click or drag-drop command

                _mm.MapLongClicked(x, y);

                //TODO without looking at the source of drag, it would just move the active army instead of actual dragged army

            }

            //timing specific code, is that view or viewModel responsibility? 

            var span = DateTime.Now.Subtract(mouseDownTime);
            if (span.TotalMilliseconds < 400)  //lol
            {
                _mm.MapClicked(x, y);
                //try without image prop event
                mouseDownTime = DateTime.MinValue;
            } else
            {
                _mm.MapLongClicked(x, y);
                mouseDownTime = DateTime.MinValue;
            }


        }



    }

}
