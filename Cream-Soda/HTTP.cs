using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace CreamSoda
{
  internal class HTTP
  {
    private WebClient m_client;

    public HTTP()
    {
      this.m_client = new WebClient();
    }

    public bool StartDownload(AsyncCompletedEventHandler dlFinishedCallback, DownloadProgressChangedEventHandler dlProgressCallback, string URL, string SavePath)
    {
      Uri address;
      try
      {
        address = new Uri(URL);
      }
      catch (Exception)
            {
        return false;
      }
      this.m_client.DownloadFileCompleted += dlFinishedCallback;
      this.m_client.DownloadProgressChanged += dlProgressCallback;
      int length = SavePath.LastIndexOf("/");
      if (length == -1)
        length = SavePath.LastIndexOf("\\");
      Directory.CreateDirectory(SavePath.Substring(0, length));
      this.m_client.DownloadFileAsync(address, MyToolkit.ValidPath(SavePath));
      return true;
    }

    public void CancelDownload()
    {
      this.m_client.CancelAsync();
    }

    public bool Active
    {
      get
      {
        return this.m_client.IsBusy;
      }
    }

    public long Length
    {
      get
      {
        try
        {
          return Convert.ToInt64(this.m_client.ResponseHeaders["Content-Length"]);
        }
        catch (Exception)
                {
          return 0;
        }
      }
    }
  }
}
