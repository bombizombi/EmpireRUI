using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire.ViewModels
{
    public class ProductionViewModel : INotifyPropertyChanged
    {
        public ProductionViewModel(MapViewModel mvm)
        {
            //needs city spec as well

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool[] _boolArray = new bool[] { 
            true, 
            false, 
            false,
            false,
            false,
            false,
            false,
            false
        };
        public bool[] UnitsArray
        {
            get { return _boolArray; }
        }
        public int SelectedUnit
        {
            get { return Array.IndexOf(_boolArray, true); }
        }


    }
}
