using System;
using System.IO;
using System.Windows.Forms;
using CreamSoda;

internal class MyToolkit
{
  public static string[] args;

  public static string AllArgs()
  {
    string str1 = "";
    foreach (string str2 in MyToolkit.args)
      str1 = str1 + str2 + " ";
    return str1.Trim();
  }

  public static string ValidPath(string Path)
  {
    Path = !Path.Contains(":\\") ? Path.Replace("\\", "/") : Path.Replace("/", "\\");
    return Path;
  }

  public static int MinMax(int Val, int Min, int Max)
  {
    if (Val > Max)
      return Max;
    if (Val < Min)
      return Min;
    return Val;
  }

  public static bool InstallDirSafe(string path)
  {
    try
    {
      File.Move(Path.Combine(path, "CreamSoda.exe"), Path.Combine(path, "CreamSoda_rename.exe"));
      if (!File.Exists(Path.Combine(path, "CreamSoda_rename.exe")))
        return false;
      File.Move(Path.Combine(path, "CreamSoda_rename.exe"), Path.Combine(path, "CreamSoda.exe"));
      return true;
    }
    catch (Exception)
        {
      return false;
    }
  }

  public static bool CreateShortcut(string LinkPathName, string TargetPathName)
  {
    return true;
  }

  public static void ErrorReporter(Exception ex, string source)
  {
    int num = (int) MessageBox.Show(ex.Message, source);
  }

  public static void ActivityLog(string Line)
  {
    try
    {
      using (StreamWriter streamWriter = new StreamWriter(Path.Combine(Settings.GamePath, "CreamSodaActivityLog.txt"), true))
        streamWriter.WriteLine("[" + DateTime.Now.ToString() + "]\t" + Line);
    }
    catch (Exception)
        {
    }
  }
}
