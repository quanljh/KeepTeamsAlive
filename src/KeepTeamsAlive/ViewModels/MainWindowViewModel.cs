using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using KeepTeamsAlive.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace KeepTeamsAlive.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactivePropertySlim<DateTime> SelectedTime { get; set; }

    public ReactivePropertySlim<string> CountDownTimeText { get; set; }

    public ReactivePropertySlim<bool> CountDownTimeTextVisible { get; set; }

    public ReactivePropertySlim<string> Message { get; set; }

    public ReactiveCommandSlim StartCountDownCommand { get; set; }

    public ReactiveCommandSlim StopCountDownCommand { get; set; }

    public ReactiveCommandSlim LoadWindowCommand { get; set; }

    public ReactiveCommandSlim CloseWindowCommand { get; set; }

    public Action<string, string> ShowMessageBox { get; set; }

    public Action CloseWindowAction { get; set; }

    public Action<bool> CountTimerControl { get; set; }

    public Action<bool> TimerControl { get; set; }

    public Action CloseWindow { get; set; }

    private IntPtr _teamsHwnd;

    private bool _isTeamsRunning;

    private Process[] _processes { get; set; }


    public MainWindowViewModel()
    {
        InitializeReactiveProperties();

        InitializeReactiveCommands();

    }

    private void InitializeReactiveProperties()
    {
        SelectedTime = new ReactivePropertySlim<DateTime>(DateTime.Now.AddHours(1)).AddTo(Disposables);
        CountDownTimeText = new ReactivePropertySlim<string>().AddTo(Disposables);
        CountDownTimeTextVisible = new ReactivePropertySlim<bool>(false).AddTo(Disposables);
        Message = new ReactivePropertySlim<string>("Close this application if you want to back to work.").AddTo(Disposables);
    }

    private void InitializeReactiveCommands()
    {
        StartCountDownCommand = new ReactiveCommandSlim().WithSubscribe(() =>
        {
            if (SelectedTime.Value.TimeOfDay < DateTime.Now.TimeOfDay)
            {
                ShowMessageBox.Invoke("Cannot set a time before current date time", "Error");
                return;
            }
            CountTimerControl.Invoke(true);
            CountDownTimeText.Value = (SelectedTime.Value.TimeOfDay - DateTime.Now.TimeOfDay).ToString(@"hh\:mm\:ss");
            CountDownTimeTextVisible.Value = true;
        }).AddTo(Disposables);

        StopCountDownCommand = new ReactiveCommandSlim().WithSubscribe(() =>
        {
            CountTimerControl.Invoke(false);
            CountDownTimeTextVisible.Value = false;
        });

        LoadWindowCommand = new ReactiveCommandSlim().WithSubscribe(() =>
        {
            _processes = Process.GetProcessesByName("Teams");

            if (_processes.Length == 0)
            {
                _processes = Process.GetProcessesByName("ms-teams");

                if (_processes.Length == 0)
                {
                    _isTeamsRunning = false;
                    ShowMessageBox.Invoke("Teams is not running!", "Warning");
                    CloseWindow.Invoke();
                    return;
                }
            }

            _isTeamsRunning = true;

            _teamsHwnd = _processes[0].MainWindowHandle;

            var windowPlacement = new WINDOWPLACEMENT
            {
                length = Marshal.SizeOf<WINDOWPLACEMENT>()
            };
            if (NativeMethods.GetWindowPlacement(_teamsHwnd, ref windowPlacement))
            {
                if (windowPlacement.showCmd == ShowWindowCommands.ShowMinimized)
                {
                    NativeMethods.ShowWindow(_teamsHwnd, (int)ShowWindowCommands.Normal);
                }
                else
                {
                    NativeMethods.ShowWindow(_teamsHwnd, (int)ShowWindowCommands.ShowDefault);
                }
            }

            if (_teamsHwnd == IntPtr.Zero)
            {
                Message.Value = "Starting teams...";
                Process process = new Process();
                process.StartInfo.FileName = "ms-teams.exe";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
                Message.Value = "Close this application if you want to back to work.";
            }

            NativeMethods.SetForegroundWindow(_teamsHwnd);


            //TimerControl.Invoke(true);
        }).AddTo(Disposables);

        CloseWindowCommand = new ReactiveCommandSlim().WithSubscribe(() =>
        {
            if (!_isTeamsRunning)
            {
                return;
            }

            TimerControl.Invoke(false);
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);

            Disposables.Dispose();
        }).AddTo(Disposables);
    }

    public void CountTimerEventHandler()
    {
        var timeSpan = (SelectedTime.Value.TimeOfDay - DateTime.Now.TimeOfDay);
        if (timeSpan <= TimeSpan.Zero)
        {
            if (_processes.Length > 0)
            {
                foreach (var process in _processes)
                {
                    process.Kill(true);
                }
            }
            CloseWindow.Invoke();
        }
        CountDownTimeText.Value = timeSpan.ToString(@"hh\:mm\:ss");

    }

    public void TimerEventHandler()
    {
        NativeMethods.SendKeyboardInput(_teamsHwnd, Key.F15);

        // Set new state to prevent system sleep
        var fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED | NativeMethods.ES_DISPLAY_REQUIRED);
        if (fPreviousExecutionState == 0)
        {
            Debug.WriteLine("SetThreadExecutionState failed. Do something here...");
        }
    }
}