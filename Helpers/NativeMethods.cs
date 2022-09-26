using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Input;

namespace KeepTeamsAlive.Helpers;

public static class NativeMethods
{
    public const uint ES_CONTINUOUS = 0x80000000;
    public const uint ES_SYSTEM_REQUIRED = 0x00000001;
    public const uint ES_DISPLAY_REQUIRED = 0x00000002;

    // Import SetThreadExecutionState Win32 API and necessary flags
    [DllImport("kernel32.dll")]
    public static extern uint SetThreadExecutionState(uint esFlags);

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowplacement
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetWindowPlacement(IntPtr hWnd, [In, Out] ref WINDOWPLACEMENT lpwndpl);

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowplacement
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    internal static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

    [DllImport("user32.Dll", CharSet = CharSet.Auto)]
    internal static extern void GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    // https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr voidProcessId);

    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-attachthreadinput
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool AttachThreadInput([In] uint idAttach, [In] uint idAttachTo, [In, MarshalAs(UnmanagedType.Bool)] bool fAttach);

    [ResourceExposure(ResourceScope.None)]
    [DllImport("User32", ExactSpelling = true, CharSet = CharSet.Auto)]
    internal static extern IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags flags);

    [DllImport("user32.dll")]
    internal static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    internal static extern IntPtr SetFocus(IntPtr hWnd);

    //https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-sendinput
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint SendInput(uint nInputs, [In, MarshalAs(UnmanagedType.LPArray)] INPUT[] pInputs, int cbSize);


    public static bool SendKeyboardInput(IntPtr hWnd, Key key, Key[] modifiers = null, int delay = 0)
    {
        /* delay param not used here */
        uint targetThreadID = GetWindowThreadProcessId(hWnd, IntPtr.Zero);
        uint currentThreadID = GetCurrentThreadId();

        if (targetThreadID != currentThreadID)
        {
            try
            {
                if (!AttachThreadInput(currentThreadID, targetThreadID, true)) return false;
                var parentWindow = GetAncestor(hWnd, GetAncestorFlags.GA_ROOT);
                if (IsIconic(parentWindow))
                {
                    if (!RestoreWindow(parentWindow)) return false;
                }

                if (!BringWindowToTop(parentWindow)) return false;
                if (SetFocus(hWnd) == IntPtr.Zero) return false;
            }
            finally
            {
                AttachThreadInput(currentThreadID, targetThreadID, false);
            }
        }
        else
        {
            SetFocus(hWnd);
        }

        var flagsKeyDw = IsExtendedKey(key) ? KeyboardInputFlags.ExtendedKey : KeyboardInputFlags.KeyDown;
        var flagsKeyUp = KeyboardInputFlags.KeyUp | (IsExtendedKey(key) ? KeyboardInputFlags.ExtendedKey : 0);

        var inputs = new List<INPUT>();
        var input = new INPUT(SendInputType.InputKeyboard);

        // Key Modifiers Down
        if (!(modifiers is null))
        {
            foreach (var modifier in modifiers)
            {
                input.Union.Keyboard.Flags = KeyboardInputFlags.KeyDown;
                input.Union.Keyboard.VirtKeys = (ushort)KeyInterop.VirtualKeyFromKey(modifier);
                inputs.Add(input);
            }
        }

        // Key Down
        input.Union.Keyboard.Flags = flagsKeyDw | KeyboardInputFlags.Unicode;
        input.Union.Keyboard.VirtKeys = (ushort)KeyInterop.VirtualKeyFromKey(key);
        inputs.Add(input);

        // Key Up
        input.Union.Keyboard.Flags = flagsKeyUp | KeyboardInputFlags.Unicode;
        input.Union.Keyboard.VirtKeys = (ushort)KeyInterop.VirtualKeyFromKey(key);
        inputs.Add(input);

        // Key Modifiers Up
        if (!(modifiers is null))
        {
            foreach (var modifier in modifiers)
            {
                input.Union.Keyboard.Flags = KeyboardInputFlags.KeyUp;
                input.Union.Keyboard.VirtKeys = (ushort)KeyInterop.VirtualKeyFromKey(modifier);
                inputs.Add(input);
            }
        }

        uint sent = SendInput((uint)inputs.Count(), inputs.ToArray(), Marshal.SizeOf<INPUT>());
        return sent > 0;
    }

    private static readonly Key[] ExtendedKeys = { Key.Up, Key.Down, Key.Left, Key.Right, Key.Home, Key.End, Key.Prior, Key.Next, Key.Insert, Key.Delete };
    private static bool IsExtendedKey(Key key) => ExtendedKeys.Contains(key);

    public static bool RestoreWindow(IntPtr hWnd)
    {
        var wpl = new WINDOWPLACEMENT()
        {
            length = Marshal.SizeOf<WINDOWPLACEMENT>()
        };
        if (!GetWindowPlacement(hWnd, ref wpl)) return false;

        wpl.flags = WplFlags.WPF_ASYNCWINDOWPLACEMENT;
        wpl.showCmd = ShowWindowCommands.Restore;
        return SetWindowPlacement(hWnd, ref wpl);
    }
}