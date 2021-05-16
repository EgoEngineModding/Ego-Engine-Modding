using EgoErpArchiver.ViewModel;
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
            var mainVM = (MainViewModel)Current.Resources["MainVM"];
            Current.Resources.Add("CommandLineArgs", e.Args);
            mainVM.ParseCommandLineArguments();

            var wnd = new MainWindow();
            wnd.Show();
        }
    }
}
