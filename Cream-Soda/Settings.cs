using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CreamSoda
{
  internal class Settings
  {
    public static bool SetupNeeded
    {
      get
      {
        if (!(Settings.GamePath == ""))
          return !File.Exists(Path.Combine(Settings.GamePath, "CreamSoda.exe"));
        return true;
      }
    }

    public static string GamePath
    {
      get
      {
        Settings.FixRegistryPolution();
        return Settings.CreamSodaRegistry.GetValue("CoHPath", "").ToString();
      }
      set
      {
        Settings.CreamSodaRegistry.SetValue("CoHPath", value);
      }
    }

    public static bool QuitOnLaunch
    {
      get
      {
        return Settings.CreamSodaRegistry.GetValue(nameof (QuitOnLaunch), "FALSE").ToString().ToUpper() == "TRUE";
      }
      set
      {
        if (value)
          Settings.CreamSodaRegistry.SetValue(nameof (QuitOnLaunch), "TRUE");
        else
          Settings.CreamSodaRegistry.SetValue(nameof (QuitOnLaunch), "FALSE");
      }
    }

    public static string GameParams
    {
      get
      {
        return Settings.CreamSodaRegistry.GetValue("Parameters", "").ToString();
      }
      set
      {
        Settings.CreamSodaRegistry.SetValue("Parameters", value);
      }
    }

    public static Color BGColor
    {
      get
      {
        int result;
        if (int.TryParse(Settings.CreamSodaRegistry.GetValue(nameof (BGColor), System.Drawing.SystemColors.Control.ToArgb()).ToString(), out result))
          return Color.FromArgb(result);
        return Color.Black;
      }
      set
      {
        Settings.CreamSodaRegistry.SetValue(nameof (BGColor), value.ToArgb());
      }
    }

    public static Color TextColor
    {
      get
      {
        int result;
        if (int.TryParse(Settings.CreamSodaRegistry.GetValue(nameof (TextColor), System.Drawing.SystemColors.InfoText.ToArgb()).ToString(), out result))
          return Color.FromArgb(result);
        return Color.Black;
      }
      set
      {
        Settings.CreamSodaRegistry.SetValue(nameof (TextColor), value.ToArgb());
      }
    }

    public static List<string> Manifests
    {
      get
      {
        return ((IEnumerable<string>) Settings.CreamSodaRegistry.GetValue(nameof (Manifests), "").ToString().Split(new char[1]
        {
          '\n'
        }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
      }
      set
      {
        string str1 = "";
        foreach (string str2 in value)
          str1 = str1 + str2.Trim() + "\n";
        if (str1.EndsWith("\n"))
          str1 = str1.Substring(0, str1.Length - 1);
        Settings.CreamSodaRegistry.SetValue(nameof (Manifests), str1);
      }
    }

    public static string LastManifest
    {
      get
      {
        return Settings.CreamSodaRegistry.GetValue(nameof (LastManifest), "").ToString();
      }
      set
      {
        Settings.CreamSodaRegistry.SetValue(nameof (LastManifest), value);
      }
    }

    public static void Reset()
    {
      Settings.CreamSodaRegistry.DeleteValue("CoHPath");
    }

    private static RegistryKey CreamSodaRegistry
    {
      get
      {
        return Registry.CurrentUser.OpenSubKey("Software\\CreamSoda\\Settings", true) ?? Registry.CurrentUser.CreateSubKey("Software\\CreamSoda\\Settings");
      }
    }

    private static void FixRegistryPolution()
    {
      try
      {
        string str = Registry.CurrentUser.GetValue("CoHPath", "").ToString();
        if (!(str != ""))
          return;
        Settings.GamePath = str;
        Registry.CurrentUser.DeleteValue("CoHPath");
      }
      catch (Exception)
            {
      }
    }
  }
}
