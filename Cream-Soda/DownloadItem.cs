using System.Collections;

namespace CreamSoda
{
  internal class DownloadItem
  {
    private ArrayList m_urls;
    private string m_filePath;

    public DownloadItem(ArrayList URL, string filePath)
    {
      this.m_urls = URL;
      this.m_filePath = MyToolkit.ValidPath(filePath);
    }

    public ArrayList URLs
    {
      get
      {
        return this.m_urls;
      }
      set
      {
        this.m_urls = this.URLs;
      }
    }

    public string FilePath
    {
      get
      {
        return this.m_filePath;
      }
      set
      {
        this.m_filePath = MyToolkit.ValidPath(this.FilePath);
      }
    }
  }
}
