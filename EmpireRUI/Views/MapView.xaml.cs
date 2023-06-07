﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
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

/// <summary>
/// Interaction logic for MapView.xaml
/// </summary>
public partial class MapView : MapViewBase
{
    public MapView()
    {
        InitializeComponent();

        this
            .WhenActivated(
                disposables =>
                {
                    this
                        .OneWayBind(this.ViewModel, vm => vm.MapString, v => v.tbMap.Text)
                        .DisposeWith(disposables);
                });

        //22
        this
            .WhenActivated(
                disposables =>
                {
                    _ = ViewModel?.StartMainGameLoop();
                });

    }



} //end class MapView



//workaround for XAML inabillity to inherit generic class
public class MapViewBase : ReactiveUserControl<MapViewModel> { }
