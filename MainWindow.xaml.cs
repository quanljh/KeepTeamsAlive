using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using KeepTeamsAlive.Helpers;

namespace KeepTeamsAlive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        private IntPtr _teamsHWND;

        private bool _isTeamsRunning;

        public MainWindow()
        {
            InitializeComponent();

            var processes = Process.GetProcessesByName("Teams");

            if (processes.Length == 0)
            {
                processes = Process.GetProcessesByName("ms-teams");

                if (processes.Length == 0)
                {
                    _isTeamsRunning = false;
                    MessageBox.Show("Teams is not running!", "Warning");
                    Close();
                    return;
                }
            }

            _isTeamsRunning = true;

            _teamsHWND = processes[0].MainWindowHandle;

            var windowPlacement = new WINDOWPLACEMENT
            {
                length = Marshal.SizeOf<WINDOWPLACEMENT>()
            };
            if (NativeMethods.GetWindowPlacement(_teamsHWND, ref windowPlacement))
            {
                if (windowPlacement.showCmd == ShowWindowCommands.ShowMinimized)
                {
                    NativeMethods.ShowWindow(_teamsHWND, (int)ShowWindowCommands.Normal);
                }
                else
                {
                    NativeMethods.ShowWindow(_teamsHWND, (int)ShowWindowCommands.ShowDefault);
                }
            }

        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            if (!_isTeamsRunning)
            {
                return;
            }
            _timer.Stop();
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isTeamsRunning)
            {
                return;
            }


            if (_teamsHWND == IntPtr.Zero)
            {
                Message.Text = "Starting teams...";
                Process process = new Process();
                process.StartInfo.FileName = "ms-teams.exe";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
                Message.Text = "Close this application if you want to back to work.";
            }

            NativeMethods.SetForegroundWindow(_teamsHWND);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(4)
            };


            _timer.Tick += (o, args) =>
            {
                NativeMethods.SendKeyboardInput(_teamsHWND, Key.F15);

                // Set new state to prevent system sleep
                var fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED | NativeMethods.ES_DISPLAY_REQUIRED);
                if (fPreviousExecutionState == 0)
                {
                    Debug.WriteLine("SetThreadExecutionState failed. Do something here...");
                }
            };

            _timer.Start();
        }
    }
}
