public class DownloadURL
{
  protected string m_URL = "";
  protected int m_PullCount;

  public DownloadURL(string URL)
  {
    this.m_URL = URL.Trim();
    this.m_PullCount = 0;
  }

  public string URL
  {
    get
    {
      return this.URL;
    }
  }

  public int PullCount
  {
    get
    {
      return this.m_PullCount;
    }
  }

  public string PullURL()
  {
    ++this.m_PullCount;
    return this.m_URL;
  }
}
