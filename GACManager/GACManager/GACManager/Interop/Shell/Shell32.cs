using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GACManager.Interop.Shell
{

public static class Shell32
{

    public static void ShowFileProperties(string Filename)
    {
        SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
        info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
        info.lpVerb = "properties";
        info.lpFile = Filename;
        info.nShow = SW_SHOW;
        info.fMask = SEE_MASK_INVOKEIDLIST;
        ShellExecuteEx(ref info);
    }

private const int SW_SHOW = 5;
private const uint SEE_MASK_INVOKEIDLIST = 12;

[DllImport("shell32.dll")]
static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

}


[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]

public struct SHELLEXECUTEINFO
{

    public int cbSize;

    public uint fMask;

    public IntPtr hwnd;

    public String lpVerb;

    public String lpFile;

    public String lpParameters;

    public String lpDirectory;

    public int nShow;

    public IntPtr hInstApp;

    public IntPtr lpIDList;

    public String lpClass;

    public IntPtr hkeyClass;

    public uint dwHotKey;

    public IntPtr hIcon;

    public IntPtr hProcess;

}
}
