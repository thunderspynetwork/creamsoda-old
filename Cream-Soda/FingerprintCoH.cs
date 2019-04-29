using System.IO;
using System.Security.Cryptography;
using System.Text;

public class FingerprintCoH
{
  private string m_FileName;
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

  public FingerprintCoH(string FileName)
  {
    this.m_FileName = !FileName.Contains(":\\") ? FileName.Replace("\\", "/") : FileName.Replace("/", "\\");
    this.m_Size = new FileInfo(FileName).Length;
    this.m_Checksum = this.GenerateHash();
  }

  public FingerprintCoH(string FileName, string Checksum)
  {
    this.m_FileName = !FileName.Contains(":\\") ? FileName.Replace("\\", "/") : FileName.Replace("/", "\\");
    this.m_Size = new FileInfo(FileName).Length;
    this.m_Checksum = Checksum.ToUpper();
  }

  public FingerprintCoH(string FileName, int Size, string Checksum)
  {
    this.m_FileName = !FileName.Contains(":\\") ? FileName.Replace("\\", "/") : FileName.Replace("/", "\\");
    this.m_Checksum = Checksum.ToUpper();
    this.m_Size = Size;
  }

  public bool Equals(Fingerprint other)
  {
    return !(this.FileName != other.FileName) && this.Size == other.Size && !(this.Checksum != other.Checksum);
  }

  public string GenerateHash(string path)
  {
    byte[] hash = MD5.Create().ComputeHash(File.ReadAllBytes(path));
    StringBuilder stringBuilder1 = new StringBuilder();
    StringBuilder stringBuilder2 = new StringBuilder();
    StringBuilder stringBuilder3 = new StringBuilder();
    StringBuilder stringBuilder4 = new StringBuilder();
    for (int index = 4; index > 0; --index)
    {
      stringBuilder1.Append(hash[index + 0 - 1].ToString("x2"));
      stringBuilder2.Append(hash[index + 4 - 1].ToString("x2"));
      stringBuilder3.Append(hash[index + 8 - 1].ToString("x2"));
      stringBuilder4.Append(hash[index + 12 - 1].ToString("x2"));
    }
    stringBuilder1.Append(" ");
    stringBuilder2.Append(" ");
    stringBuilder3.Append(" ");
    StringBuilder stringBuilder5 = new StringBuilder();
    stringBuilder5.Append(stringBuilder1);
    stringBuilder5.Append(stringBuilder2);
    stringBuilder5.Append(stringBuilder3);
    stringBuilder5.Append(stringBuilder4);
    return stringBuilder5.ToString().ToUpper();
  }

  public string GenerateHash()
  {
    return this.GenerateHash(this.FileName);
  }
}
