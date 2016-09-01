using EgoErpArchiver.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EgoErpArchiver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainViewModel mainVM = (MainViewModel)Application.Current.Resources["MainVM"];
            Application.Current.Resources.Add("CommandLineArgs", e.Args);
            mainVM.ParseCommandLineArguments();

            MainWindow wnd = new MainWindow();
            wnd.Show();
        }
    }
}
