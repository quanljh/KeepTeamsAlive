using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace MessAround
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int EXIT_CODE_ALREADY_RUNNING = -9;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (Process.GetProcessesByName(Assembly.GetExecutingAssembly().GetName().Name).Length > 1)
            {
                MessageBox.Show("Cannot run this application multiple times", "Error");
                Current.Shutdown(EXIT_CODE_ALREADY_RUNNING);
                return;
            }
            base.OnStartup(e);
        }
    }
}
