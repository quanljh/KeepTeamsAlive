using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace KeepTeamsAlive.Helpers;

public enum ShowWindowCommands
{
    /// <summary>
    /// Hides the window and activates another window.
    /// </summary>
    Hide = 0,
    /// <summary>
    /// Activates and displays a window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when displaying the window
    /// for the first time.
    /// </summary>
    Normal = 1,
    /// <summary>
    /// Activates the window and displays it as a minimized window.
    /// </summary>
    ShowMinimized = 2,
    /// <summary>
    /// Maximizes the specified window.
    /// </summary>
    Maximize = 3, // is this the right value?
    /// <summary>
    /// Activates the window and displays it as a maximized window.
    /// </summary>      
    ShowMaximized = 3,
    /// <summary>
    /// Displays a window in its most recent size and position. This value
    /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
    /// the window is not activated.
    /// </summary>
    ShowNoActivate = 4,
    /// <summary>
    /// Activates the window and displays it in its current size and position.
    /// </summary>
    Show = 5,
    /// <summary>
    /// Minimizes the specified window and activates the next top-level
    /// window in the Z order.
    /// </summary>
    Minimize = 6,
    /// <summary>
    /// Displays the window as a minimized window. This value is similar to
    /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
    /// window is not activated.
    /// </summary>
    ShowMinNoActive = 7,
    /// <summary>
    /// Displays the window in its current size and position. This value is
    /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
    /// window is not activated.
    /// </summary>
    ShowNA = 8,
    /// <summary>
    /// Activates and displays the window. If the window is minimized or
    /// maximized, the system restores it to its original size and position.
    /// An application should specify this flag when restoring a minimized window.
    /// </summary>
    Restore = 9,
    /// <summary>
    /// Sets the show state based on the SW_* value specified in the
    /// STARTUPINFO structure passed to the CreateProcess function by the
    /// program that started the application.
    /// </summary>
    ShowDefault = 10,
    /// <summary>
    ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
    /// that owns the window is not responding. This flag should only be
    /// used when minimizing windows from a different thread.
    /// </summary>
    ForceMinimize = 11
}

// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input
[StructLayout(LayoutKind.Sequential)]
public struct INPUT
{
    public SendInputType InputType;
    public InputUnion Union;

    public INPUT(SendInputType type)
    {
        InputType = type;
        Union = new InputUnion();
    }
}

public enum SendInputType : uint
{
    InputMouse = 0,
    InputKeyboard = 1,
    InputHardware = 2
}

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)]
    public MOUSEINPUT Mouse;

    [FieldOffset(0)]
    public KEYBDINPUT Keyboard;

    [FieldOffset(0)]
    public HARDWAREINPUT Hardware;
}

[StructLayout(LayoutKind.Sequential)]
public struct MOUSEINPUT
{
    public int dx;
    public int dy;
    public uint mouseData;
    public MouseEventdwFlags dwFlags;
    public uint time;
    public IntPtr dwExtraInfo;
}

// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput
[StructLayout(LayoutKind.Sequential)]
public struct KEYBDINPUT
{
    public ushort VirtKeys;
    public ushort wScan;
    public KeyboardInputFlags Flags;
    public uint time;
    public IntPtr dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
public struct HARDWAREINPUT
{
    public int uMsg;
    public short wParamL;
    public short wParamH;
}

[Flags]
public enum MouseEventdwFlags : uint
{
    MOUSEEVENTF_MOVE = 0x0001,
    MOUSEEVENTF_LEFTDOWN = 0x0002,
    MOUSEEVENTF_LEFTUP = 0x0004,
    MOUSEEVENTF_RIGHTDOWN = 0x0008,
    MOUSEEVENTF_RIGHTUP = 0x0010,
    MOUSEEVENTF_MIDDLEDOWN = 0x0020,
    MOUSEEVENTF_MIDDLEUP = 0x0040,
    MOUSEEVENTF_XDOWN = 0x0080,
    MOUSEEVENTF_XUP = 0x0100,
    MOUSEEVENTF_WHEEL = 0x0800,
    MOUSEEVENTF_VIRTUALDESK = 0x4000,
    MOUSEEVENTF_ABSOLUTE = 0x8000
}

[Flags]
public enum KeyboardInputFlags : uint
{
    KeyDown = 0x0000,
    ExtendedKey = 0x0001,
    KeyUp = 0x0002,
    ScanCode = 0x0008,
    Unicode = 0x0004
}

// https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-windowplacement
[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
    public int length;
    public WplFlags flags;
    public ShowWindowCommands showCmd;
    public POINT ptMinPosition;
    public POINT ptMaxPosition;
    public RECT rcNormalPosition;
}

public enum WplFlags : uint
{
    WPF_ASYNCWINDOWPLACEMENT = 0x0004,   // If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
    WPF_RESTORETOMAXIMIZED = 0x0002,     // The restored window will be maximized, regardless of whether it was maximized before it was minimized. This setting is only valid the next time the window is restored. It does not change the default restoration behavior.
                                         // This flag is only valid when the SW_SHOWMINIMIZED value is specified for the showCmd member.
    WPF_SETMINPOSITION = 0x0001          // The coordinates of the minimized window may be specified. This flag must be specified if the coordinates are set in the ptMinPosition member.
}


public enum GetAncestorFlags : uint
{
    GA_PARENT = 1,     // Retrieves the parent window.This does not include the owner, as it does with the GetParent function.
    GA_ROOT = 2,       // Retrieves the root window by walking the chain of parent windows.
    GA_ROOTOWNER = 3   // Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
}

[StructLayout(LayoutKind.Sequential)]
public class POINT
{
    public int x;
    public int y;

    public POINT() { }

    public POINT(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Point ToPoint() => new Point(this.x, this.y);
    public PointF ToPointF() => new PointF((float)this.x, (float)this.y);
    public POINT FromPoint(Point p) => new POINT(p.X, p.Y);
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        Left = left; Top = top; Right = right; Bottom = bottom;
    }

    public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
    public Rectangle ToRectangleOffset(POINT p) => Rectangle.FromLTRB(p.x, p.y, Right + p.x, Bottom + p.y);

    public RECT FromRectangle(RectangleF rectangle) => FromRectangle(Rectangle.Round(rectangle));
    public RECT FromRectangle(Rectangle rectangle) => new RECT()
    {
        Left = rectangle.Left,
        Top = rectangle.Top,
        Bottom = rectangle.Bottom,
        Right = rectangle.Right
    };
    public RECT FromXYWH(int x, int y, int width, int height) => new RECT(x, y, x + width, y + height);
}
