using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CreamSoda
{
  public class ShellLink : IDisposable
  {
    private string shortcutFile = "";
    private ShellLink.IShellLinkW linkW;
    private ShellLink.IShellLinkA linkA;

    public ShellLink()
    {
      if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        this.linkW = (ShellLink.IShellLinkW) new ShellLink.CShellLink();
      else
        this.linkA = (ShellLink.IShellLinkA) new ShellLink.CShellLink();
    }

    public ShellLink(string linkFile)
      : this()
    {
      this.Open(linkFile);
    }

    ~ShellLink()
    {
      this.Dispose();
    }

    public void Dispose()
    {
      if (this.linkW != null)
      {
        Marshal.ReleaseComObject(linkW);
        this.linkW = null;
      }
      if (this.linkA == null)
        return;
      Marshal.ReleaseComObject(linkA);
      this.linkA = null;
    }

    public string ShortCutFile
    {
      get
      {
        return this.shortcutFile;
      }
      set
      {
        this.shortcutFile = value;
      }
    }

    public string IconPath
    {
      get
      {
        StringBuilder pszIconPath = new StringBuilder(260, 260);
        int piIcon = 0;
        if (this.linkA == null)
          this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        else
          this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        return pszIconPath.ToString();
      }
      set
      {
        StringBuilder pszIconPath = new StringBuilder(260, 260);
        int piIcon = 0;
        if (this.linkA == null)
          this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        else
          this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        if (this.linkA == null)
          this.linkW.SetIconLocation(value, piIcon);
        else
          this.linkA.SetIconLocation(value, piIcon);
      }
    }

    public int IconIndex
    {
      get
      {
        StringBuilder pszIconPath = new StringBuilder(260, 260);
        int piIcon = 0;
        if (this.linkA == null)
          this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        else
          this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        return piIcon;
      }
      set
      {
        StringBuilder pszIconPath = new StringBuilder(260, 260);
        int piIcon = 0;
        if (this.linkA == null)
          this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        else
          this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
        if (this.linkA == null)
          this.linkW.SetIconLocation(pszIconPath.ToString(), value);
        else
          this.linkA.SetIconLocation(pszIconPath.ToString(), value);
      }
    }

    public string Target
    {
      get
      {
        StringBuilder pszFile = new StringBuilder(260, 260);
        if (this.linkA == null)
        {
          ShellLink._WIN32_FIND_DATAW pfd = new ShellLink._WIN32_FIND_DATAW();
          this.linkW.GetPath(pszFile, pszFile.Capacity, ref pfd, 2U);
        }
        else
        {
          ShellLink._WIN32_FIND_DATAA pfd = new ShellLink._WIN32_FIND_DATAA();
          this.linkA.GetPath(pszFile, pszFile.Capacity, ref pfd, 2U);
        }
        return pszFile.ToString();
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetPath(value);
        else
          this.linkA.SetPath(value);
      }
    }

    public string WorkingDirectory
    {
      get
      {
        StringBuilder pszDir = new StringBuilder(260, 260);
        if (this.linkA == null)
          this.linkW.GetWorkingDirectory(pszDir, pszDir.Capacity);
        else
          this.linkA.GetWorkingDirectory(pszDir, pszDir.Capacity);
        return pszDir.ToString();
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetWorkingDirectory(value);
        else
          this.linkA.SetWorkingDirectory(value);
      }
    }

    public string Description
    {
      get
      {
        StringBuilder pszFile = new StringBuilder(1024, 1024);
        if (this.linkA == null)
          this.linkW.GetDescription(pszFile, pszFile.Capacity);
        else
          this.linkA.GetDescription(pszFile, pszFile.Capacity);
        return pszFile.ToString();
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetDescription(value);
        else
          this.linkA.SetDescription(value);
      }
    }

    public string Arguments
    {
      get
      {
        StringBuilder pszArgs = new StringBuilder(260, 260);
        if (this.linkA == null)
          this.linkW.GetArguments(pszArgs, pszArgs.Capacity);
        else
          this.linkA.GetArguments(pszArgs, pszArgs.Capacity);
        return pszArgs.ToString();
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetArguments(value);
        else
          this.linkA.SetArguments(value);
      }
    }

    public ShellLink.LinkDisplayMode DisplayMode
    {
      get
      {
        uint piShowCmd = 0;
        if (this.linkA == null)
          this.linkW.GetShowCmd(out piShowCmd);
        else
          this.linkA.GetShowCmd(out piShowCmd);
        return (ShellLink.LinkDisplayMode) piShowCmd;
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetShowCmd((uint) value);
        else
          this.linkA.SetShowCmd((uint) value);
      }
    }

    public Keys HotKey
    {
      get
      {
        short pwHotkey = 0;
        if (this.linkA == null)
          this.linkW.GetHotkey(out pwHotkey);
        else
          this.linkA.GetHotkey(out pwHotkey);
        return (Keys) pwHotkey;
      }
      set
      {
        if (this.linkA == null)
          this.linkW.SetHotkey((short) value);
        else
          this.linkA.SetHotkey((short) value);
      }
    }

    public void Save()
    {
      this.Save(this.shortcutFile);
    }

    public void Save(string linkFile)
    {
      if (this.linkA == null)
      {
        ((ShellLink.IPersistFile) this.linkW).Save(linkFile, true);
        this.shortcutFile = linkFile;
      }
      else
      {
        ((ShellLink.IPersistFile) this.linkA).Save(linkFile, true);
        this.shortcutFile = linkFile;
      }
    }

    public void Open(string linkFile)
    {
      this.Open(linkFile, IntPtr.Zero, ShellLink.EShellLinkResolveFlags.SLR_ANY_MATCH | ShellLink.EShellLinkResolveFlags.SLR_NO_UI, 1);
    }

    public void Open(string linkFile, IntPtr hWnd, ShellLink.EShellLinkResolveFlags resolveFlags)
    {
      this.Open(linkFile, hWnd, resolveFlags, 1);
    }

    public void Open(string linkFile, IntPtr hWnd, ShellLink.EShellLinkResolveFlags resolveFlags, ushort timeOut)
    {
      uint fFlags = (resolveFlags & ShellLink.EShellLinkResolveFlags.SLR_NO_UI) != ShellLink.EShellLinkResolveFlags.SLR_NO_UI ? (uint) resolveFlags : (uint) (resolveFlags | (ShellLink.EShellLinkResolveFlags) (timeOut << 16));
      if (this.linkA == null)
      {
        ((ShellLink.IPersistFile) this.linkW).Load(linkFile, 0U);
        this.linkW.Resolve(hWnd, fFlags);
        this.shortcutFile = linkFile;
      }
      else
      {
        ((ShellLink.IPersistFile) this.linkA).Load(linkFile, 0U);
        this.linkA.Resolve(hWnd, fFlags);
        this.shortcutFile = linkFile;
      }
    }

    [Guid("0000010C-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    private interface IPersist
    {
      [MethodImpl(MethodImplOptions.PreserveSig)]
      void GetClassID(out Guid pClassID);
    }

    [Guid("0000010B-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    private interface IPersistFile
    {
      [MethodImpl(MethodImplOptions.PreserveSig)]
      void GetClassID(out Guid pClassID);

      void IsDirty();

      void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

      void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

      void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

      void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }

    [Guid("000214EE-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    private interface IShellLinkA
    {
      void GetPath([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszFile, int cchMaxPath, ref ShellLink._WIN32_FIND_DATAA pfd, uint fFlags);

      void GetIDList(out IntPtr ppidl);

      void SetIDList(IntPtr pidl);

      void GetDescription([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszFile, int cchMaxName);

      void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

      void GetWorkingDirectory([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszDir, int cchMaxPath);

      void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

      void GetArguments([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszArgs, int cchMaxPath);

      void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

      void GetHotkey(out short pwHotkey);

      void SetHotkey(short pwHotkey);

      void GetShowCmd(out uint piShowCmd);

      void SetShowCmd(uint piShowCmd);

      void GetIconLocation([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

      void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

      void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);

      void Resolve(IntPtr hWnd, uint fFlags);

      void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
    }

    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    private interface IShellLinkW
    {
      void GetPath([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszFile, int cchMaxPath, ref ShellLink._WIN32_FIND_DATAW pfd, uint fFlags);

      void GetIDList(out IntPtr ppidl);

      void SetIDList(IntPtr pidl);

      void GetDescription([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszFile, int cchMaxName);

      void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

      void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszDir, int cchMaxPath);

      void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

      void GetArguments([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszArgs, int cchMaxPath);

      void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

      void GetHotkey(out short pwHotkey);

      void SetHotkey(short pwHotkey);

      void GetShowCmd(out uint piShowCmd);

      void SetShowCmd(uint piShowCmd);

      void GetIconLocation([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

      void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

      void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);

      void Resolve(IntPtr hWnd, uint fFlags);

      void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [Guid("00021401-0000-0000-C000-000000000046")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComImport]
    private class CShellLink
    {
//      [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
//      public extern CShellLink();
    }

    private enum EShellLinkGP : uint
    {
      SLGP_SHORTPATH = 1,
      SLGP_UNCPRIORITY = 2,
    }

    [Flags]
    private enum EShowWindowFlags : uint
    {
      SW_HIDE = 0,
      SW_SHOWNORMAL = 1,
      SW_NORMAL = SW_SHOWNORMAL, // 0x00000001
      SW_SHOWMINIMIZED = 2,
      SW_SHOWMAXIMIZED = SW_SHOWMINIMIZED | SW_NORMAL, // 0x00000003
      SW_MAXIMIZE = SW_SHOWMAXIMIZED, // 0x00000003
      SW_SHOWNOACTIVATE = 4,
      SW_SHOW = SW_SHOWNOACTIVATE | SW_NORMAL, // 0x00000005
      SW_MINIMIZE = SW_SHOWNOACTIVATE | SW_SHOWMINIMIZED, // 0x00000006
      SW_SHOWMINNOACTIVE = SW_MINIMIZE | SW_NORMAL, // 0x00000007
      SW_SHOWNA = 8,
      SW_RESTORE = SW_SHOWNA | SW_NORMAL, // 0x00000009
      SW_SHOWDEFAULT = SW_SHOWNA | SW_SHOWMINIMIZED, // 0x0000000A
      SW_MAX = SW_SHOWDEFAULT, // 0x0000000A
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    private struct _WIN32_FIND_DATAW
    {
      public uint dwFileAttributes;
      public ShellLink._FILETIME ftCreationTime;
      public ShellLink._FILETIME ftLastAccessTime;
      public ShellLink._FILETIME ftLastWriteTime;
      public uint nFileSizeHigh;
      public uint nFileSizeLow;
      public uint dwReserved0;
      public uint dwReserved1;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string cFileName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
      public string cAlternateFileName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct _WIN32_FIND_DATAA
    {
      public uint dwFileAttributes;
      public ShellLink._FILETIME ftCreationTime;
      public ShellLink._FILETIME ftLastAccessTime;
      public ShellLink._FILETIME ftLastWriteTime;
      public uint nFileSizeHigh;
      public uint nFileSizeLow;
      public uint dwReserved0;
      public uint dwReserved1;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
      public string cFileName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
      public string cAlternateFileName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct _FILETIME
    {
      public uint dwLowDateTime;
      public uint dwHighDateTime;
    }

    private class UnManagedMethods
    {
      [DllImport("Shell32", CharSet = CharSet.Auto)]
      internal static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)] string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);

      [DllImport("user32")]
      internal static extern int DestroyIcon(IntPtr hIcon);
    }

    [Flags]
    public enum EShellLinkResolveFlags : uint
    {
      SLR_ANY_MATCH = 2,
      SLR_INVOKE_MSI = 128, // 0x00000080
      SLR_NOLINKINFO = 64, // 0x00000040
      SLR_NO_UI = 1,
      SLR_NO_UI_WITH_MSG_PUMP = 257, // 0x00000101
      SLR_NOUPDATE = 8,
      SLR_NOSEARCH = 16, // 0x00000010
      SLR_NOTRACK = 32, // 0x00000020
      SLR_UPDATE = 4,
    }

    public enum LinkDisplayMode : uint
    {
      edmNormal = 1,
      edmMaximized = 3,
      edmMinimized = 7,
    }
  }
}
