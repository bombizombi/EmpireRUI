using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpireRUI;

public class MapViewModel : ReactiveObject, IRoutableViewModel
{
    

    public MapViewModel(IScreen screen)
    {
        HostScreen = screen;




    }
    public string UrlPathSegment { get; } = "Map";
    public IScreen HostScreen { get; }
  
}
