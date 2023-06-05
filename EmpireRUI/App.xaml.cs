﻿using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace EmpireRUI
{
    public partial class App : Application
    {
        
        //public App()
        //{
        //    Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //e.Args.  -d for debugging

            MainView mainView = new MainView();
            MainViewModel viewModel = new MainViewModel();
            mainView.DataContext = viewModel;
            mainView.Show();
        }
    }
}
