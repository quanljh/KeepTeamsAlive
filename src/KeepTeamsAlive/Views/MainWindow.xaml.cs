using System;
using System.Windows;
using System.Windows.Threading;
using KeepTeamsAlive.ViewModels;
using Quan.ControlLibrary.Controls;

namespace KeepTeamsAlive.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : QuanWindow
    {

        private DispatcherTimer _countTimer;

        private DispatcherTimer _timer;


        public MainWindow()
        {
            InitializeComponent();


            var vm = new MainWindowViewModel();
            vm.ShowMessageBox = ShowMessageBox;
            vm.CountTimerControl = CountTimerControl;
            vm.TimerControl = TimerControl;
            vm.CloseWindow = CloseWindow;
            DataContext = vm;

            _countTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _countTimer.Tick += (sender, args) => vm.CountTimerEventHandler();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(4)
            };

            _timer.Tick += (sender, args) => vm.TimerEventHandler();

        }

        private void CloseWindow()
        {
            Dispatcher.InvokeShutdown();
        }

        private void TimerControl(bool start)
        {
            if (start)
            {
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        private void CountTimerControl(bool start)
        {
            if (start)
            {
                _countTimer.Start();
            }
            else
            {
                _countTimer.Stop();
            }
        }

        private void ShowMessageBox(string text, string caption)
        {
            MessageBox.Show(text, caption);
        }
    }
}
