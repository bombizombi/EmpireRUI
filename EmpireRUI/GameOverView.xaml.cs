using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmpireRUI;

public partial class GameOverView : GameOverViewBase
{
    public GameOverView()
    {
        InitializeComponent();
    }
}


//workaround for XAML inabillity to inherit generic class
public class GameOverViewBase : ReactiveUserControl<GameOverViewModel> { }

