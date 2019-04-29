using System;
using System.IO;

namespace CreamSoda
{
  internal class DirCopy
  {
    private bool m_copySubDirs = true;
    public static bool Kill;
    private long m_FileCount;
    private long m_FilesDone;
    private bool m_Active;
    private string m_SourceDirName;
    private string m_DestDirName;

    public bool Active
    {
      get
      {
        return this.m_Active;
      }
    }

    public int Progress
    {
      get
      {
        return MyToolkit.MinMax((int) (m_FilesDone / (double) this.m_FileCount * 100.0), 0, 100);
      }
    }

    public DirCopy(string SourceDirName, string DestDirName)
    {
      this.m_SourceDirName = SourceDirName;
      this.m_DestDirName = DestDirName;
    }

    public string sourceDirName
    {
      get
      {
        return this.m_SourceDirName;
      }
    }

    public string destDirName
    {
      get
      {
        return this.m_DestDirName;
      }
    }

    public bool copySubDirs
    {
      get
      {
        return this.m_copySubDirs;
      }
    }

    public void DirectoryCopy()
    {
      this.m_Active = true;
      this.m_FileCount = this.DirectoryCount(this.sourceDirName);
      this.DirectoryCopyStep(this.sourceDirName, this.destDirName, this.copySubDirs, true);
      this.m_Active = false;
    }

    public void DirectoryCopyNoReplace()
    {
      this.m_Active = true;
      this.m_FileCount = this.DirectoryCount(this.sourceDirName);
      this.DirectoryCopyStep(this.sourceDirName, this.destDirName, this.copySubDirs, false);
      this.m_Active = false;
    }

    private void DirectoryCopyStep(string sourceDirName, string destDirName, bool copySubDirs = true, bool replace = true)
    {
      try
      {
        DirectoryInfo directoryInfo1 = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] directories = directoryInfo1.GetDirectories();
        if (!directoryInfo1.Exists)
          throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
        if (!Directory.Exists(destDirName))
          Directory.CreateDirectory(destDirName);
        foreach (FileInfo file in directoryInfo1.GetFiles())
        {
          if (DirCopy.Kill)
            return;
          string str = Path.Combine(destDirName, file.Name);
          if (File.Exists(str) & replace)
          {
            File.SetAttributes(str, File.GetAttributes(str) & ~FileAttributes.ReadOnly);
            File.Delete(str);
          }
          try
          {
            file.CopyTo(str, true);
          }
          catch (Exception)
                    {
          }
          this.m_FilesDone += file.Length;
        }
        if (!copySubDirs)
          return;
        foreach (DirectoryInfo directoryInfo2 in directories)
        {
          string destDirName1 = Path.Combine(destDirName, directoryInfo2.Name);
          this.DirectoryCopyStep(directoryInfo2.FullName, destDirName1, copySubDirs, true);
        }
      }
      catch (Exception ex)
      {
        string source = "DirCopy.DirectoryCopyStep";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private long DirectoryCount(string sourceDirName)
    {
      long num = 0;
      try
      {
        DirectoryInfo directoryInfo1 = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] directories = directoryInfo1.GetDirectories();
        foreach (FileInfo file in directoryInfo1.GetFiles())
        {
          if (DirCopy.Kill)
            return 0;
          try
          {
            num += file.Length;
          }
          catch (Exception)
                    {
          }
        }
        foreach (DirectoryInfo directoryInfo2 in directories)
          num += this.DirectoryCount(directoryInfo2.FullName);
      }
      catch (Exception ex)
      {
        string source = "DirCopy.DirectoryCopyStep";
        MyToolkit.ErrorReporter(ex, source);
      }
      return num;
    }
  }
}
