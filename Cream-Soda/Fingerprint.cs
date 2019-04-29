using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Fingerprint
{
  private static Random rand = new Random();
  private List<global::DownloadURL> m_DownloadURLs = new List<global::DownloadURL>();
  private bool m_warn = true;
  private string m_FileName;
  private string m_RootPath;
  private long m_Size;
  private string m_Checksum;
  private bool m_mismatch;

  public string FileName
  {
    get
    {
      return this.m_FileName;
    }
  }

  public string RootPath
  {
    get
    {
      return this.m_RootPath;
    }
  }

  public string FullName
  {
    get
    {
      return MyToolkit.ValidPath(Path.Combine(this.m_RootPath, this.m_FileName));
    }
  }

  public long Size
  {
    get
    {
      return this.m_Size;
    }
  }

  public string Checksum
  {
    get
    {
      return this.m_Checksum;
    }
  }

  public bool Mismatch
  {
    get
    {
      return this.m_mismatch;
    }
    set
    {
      this.m_mismatch = value;
    }
  }

  public bool Warn
  {
    get
    {
      return this.m_warn;
    }
    set
    {
      this.m_warn = this.Warn;
    }
  }

  public string DownloadURL
  {
    get
    {
      if (this.m_DownloadURLs.Count < 1)
        return "";
      int index = Fingerprint.rand.Next(0, this.m_DownloadURLs.Count);
      string str = this.m_DownloadURLs[index].PullURL();
      if (this.m_DownloadURLs[index].PullCount < 5)
        return str;
      this.m_DownloadURLs.RemoveAt(index);
      return str;
    }
  }

  public Fingerprint(string RootPath, string FileName)
  {
    try
    {
      this.m_RootPath = RootPath;
      this.m_FileName = FileName.Replace(".EXE", ".exe");
      this.m_Size = !File.Exists(this.FullName) ? 0L : new FileInfo(this.FullName).Length;
      this.m_Checksum = this.GenerateHash();
    }
    catch (Exception ex)
    {
      string source = "Fingerprint.Constructor1";
      MyToolkit.ErrorReporter(ex, source);
    }
  }

  public Fingerprint(string RootPath, string FileName, string Checksum)
  {
    try
    {
      this.m_RootPath = RootPath;
      this.m_FileName = FileName.Replace(".EXE", ".exe");
      this.m_Size = new FileInfo(FileName).Length;
      this.m_Checksum = Checksum.ToLower();
    }
    catch (Exception ex)
    {
      string source = "Fingerprint.Constructor2";
      MyToolkit.ErrorReporter(ex, source);
    }
  }

  public Fingerprint(string RootPath, string FileName, string Checksum, long Size)
  {
    try
    {
      this.m_RootPath = RootPath;
      this.m_FileName = FileName.Replace(".EXE", ".exe");
      this.m_Checksum = Checksum.ToLower();
      this.m_Size = Size;
    }
    catch (Exception ex)
    {
      string source = "Fingerprint.Constructor2";
      MyToolkit.ErrorReporter(ex, source);
    }
  }

  public void AddDownloadURL(string URL)
  {
    this.m_DownloadURLs.Add(new global::DownloadURL(URL));
  }

  public bool Equals(Fingerprint other)
  {
    return this.Size == other.Size && !(this.Checksum != other.Checksum);
  }

  public string GenerateHash(string path)
  {
    try
    {
      byte[] hash = MD5.Create().ComputeHash(File.ReadAllBytes(path));
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < hash.Length; ++index)
        stringBuilder.Append(hash[index].ToString("x2"));
      return stringBuilder.ToString().ToLower();
    }
    catch (Exception ex)
    {
      string source = "Fingerprint.GenerateHash";
      MyToolkit.ErrorReporter(ex, source);
      return "";
    }
  }

  public string GenerateHash()
  {
    return this.GenerateHash(this.FullName);
  }
}
