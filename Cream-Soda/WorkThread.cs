using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CreamSoda
{
  internal class WorkThread
  {
    private ArrayList m_ErrorLog = new ArrayList();
    private ArrayList m_WarningLog = new ArrayList();
    private ArrayList m_DownloadQueue = new ArrayList();
    private HTTP client = new HTTP();
    public string LocalManifest = "";
    public string PathRoot = "";
    public string ForumURL = "";
    private XElement LogNew = new XElement("files");
    private string m_Status = "";
    private string m_current = "";
    public static bool DontDownloadManifest;
    public static bool DontSelfUpdate;
    public static bool GenerateChecksumToClipboard;
    private ArrayList m_ManifestFileList;
    private long m_DownloadSize;
    private long m_Downloaded;
    private long m_CurrDownloadBytes;
    public string ManifestURL;
    private XElement Log;
    private int m_progress;
    private XElement m_manifest;
    public static bool Kill;
    private Thread myWorkThread;
    private bool m_DownloadActive;

    public XElement Manifest
    {
      get
      {
        return this.m_manifest;
      }
    }

    public WorkThread(string ManifestoURL)
    {
      this.ManifestURL = ManifestoURL;
    }

    public string ErrorMessage
    {
      get
      {
        string str1 = "";
        foreach (string str2 in this.m_ErrorLog)
          str1 += str2;
        return str1;
      }
    }

    public string WarningMessage
    {
      get
      {
        string str1 = "";
        foreach (string str2 in this.m_WarningLog)
          str1 += str2;
        return str1;
      }
    }

    public string Status
    {
      get
      {
        return this.m_Status;
      }
      set
      {
        this.m_Status = this.Status;
      }
    }

    public int CurProgress
    {
      get
      {
        return this.m_progress;
      }
    }

    public string CurFile
    {
      get
      {
        return this.m_current;
      }
    }

    public string LogPath
    {
      get
      {
        return Path.Combine(Settings.GamePath, "CreamSodalog.xml");
      }
    }

    public void LoadLog()
    {
      if (!File.Exists(this.LogPath))
        return;
      this.Log = XElement.Load(this.LogPath);
    }

    private void FlagVerified(string file, long size, string md5)
    {
      try
      {
        FileInfo fileInfo = new FileInfo(Path.Combine(Settings.GamePath, file));
        XElement xelement = new XElement(nameof(file));
        xelement.Add(new XAttribute("name", file));
        xelement.Add(new XAttribute(nameof(size), size));
        xelement.Add(new XAttribute(nameof(md5), md5));
        xelement.Add(new XAttribute("ModDate", fileInfo.LastWriteTime.ToString(fileInfo.LastWriteTime.ToString("yyyy.MM.dd.HH.mm.ss"))));
        this.LogNew.Add(xelement);
        bool flag = false;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while (stopwatch.ElapsedMilliseconds < 3000L)
        {
          if (!flag)
          {
            try
            {
              this.LogNew.Save(this.LogPath);
              flag = true;
            }
            catch (Exception)
                        {
              flag = false;
            }
          }
          else
            break;
        }
        if (flag)
          return;
        this.LogNew.Save(this.LogPath);
      }
      catch (Exception ex)
      {
        string source = "WorkThread.FlagVerified";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private bool AlreadyVerified(string file, long size, string md5)
    {
      try
      {
        if (!File.Exists(Path.Combine(Settings.GamePath, file)))
          return false;
        XElement xelement = this.Log.Descendants(nameof(file)).Where<XElement>(el => (string)el.Attribute("name") == file).First<XElement>();
        FileInfo fileInfo = new FileInfo(Path.Combine(Settings.GamePath, file));
        string str = xelement.Attribute("ModDate") != null ? xelement.Attribute("ModDate").Value : fileInfo.LastWriteTime.ToString("yyyy.MM.dd.HH.mm.ss");
        return fileInfo.Length == size && (!(str != fileInfo.LastWriteTime.ToString(fileInfo.LastWriteTime.ToString("yyyy.MM.dd.HH.mm.ss"))) && (long.Parse(xelement.Attribute(nameof(size)).Value) == size && xelement.Attribute(nameof(md5)).Value.ToLower() == md5));
      }
      catch (Exception)
            {
        return false;
      }
    }

    public void Validate()
    {
      long num = 0;
      this.m_Status = "Validating";
      this.LoadLog();
      foreach (Fingerprint manifestFile in this.m_ManifestFileList)
      {
        if (WorkThread.Kill)
          return;
        ++num;
        this.m_current = manifestFile.FullName;
        ProgressEventArgs progressEventArgs = new ProgressEventArgs(num, m_ManifestFileList.Count);
        if (manifestFile.Size == 0L)
        {
          if (File.Exists(manifestFile.FullName))
            File.Delete(manifestFile.FullName);
        }
        else if (File.Exists(manifestFile.FullName))
        {
          if (this.AlreadyVerified(manifestFile.FileName, manifestFile.Size, manifestFile.Checksum))
            this.FlagVerified(manifestFile.FileName, manifestFile.Size, manifestFile.Checksum);
          else if (new Fingerprint(manifestFile.RootPath, manifestFile.FileName).Equals(manifestFile))
            this.FlagVerified(manifestFile.FileName, manifestFile.Size, manifestFile.Checksum);
          else
            this.AddToDownloadQueue(manifestFile);
        }
        else
          this.AddToDownloadQueue(manifestFile);
        this.m_progress = (int) Math.Round(num / 100.0 * 100.0);
      }
      this.DownloadFiles();
    }

    private void AddToDownloadQueue(Fingerprint file)
    {
      if (file.DownloadURL != "")
      {
        this.m_DownloadQueue.Add(file);
        this.m_DownloadSize += file.Size;
      }
      else
        this.m_ErrorLog.Add("The following file has an invalid checksum. You will need to obtain it from a valid game installation:\r\n" + file.FileName + "\r\n");
    }

    public void DownloadFiles()
    {
      foreach (Fingerprint download in this.m_DownloadQueue)
      {
        if (WorkThread.Kill)
          return;
        HTTP http = new HTTP();
        bool flag = true;
        string downloadUrl = download.DownloadURL;
        while (flag)
        {
          try
          {
            MyToolkit.ActivityLog("Downloading file \"" + download.FullName + "\" from \"" + downloadUrl + "\"");
            if (http.StartDownload(new AsyncCompletedEventHandler(this.DownloadFileComplete), new DownloadProgressChangedEventHandler(this.dlProgress), downloadUrl, download.FullName + ".download"))
            {
              this.m_Status = "Downloading";
              this.m_DownloadActive = true;
            }
          }
          catch (Exception ex)
          {
            string message = ex.Message;
          }
          this.m_current = download.FullName;
          while (http.Active)
          {
            if (WorkThread.Kill)
            {
              http.CancelDownload();
              return;
            }
            Thread.Sleep(10);
          }
          Fingerprint fingerprint = new Fingerprint(download.RootPath, download.FileName + ".download");
          if (!fingerprint.Equals(download))
          {
            File.Delete(download.FullName + ".download");
            downloadUrl = download.DownloadURL;
            if (downloadUrl == "")
            {
              MyToolkit.ActivityLog("Download failed, no more URL's to try from");
              flag = false;
              string str = "Download error: " + download.FileName;
              if (fingerprint.Size == 0L)
              {
                str += "\r\nWas unable to download file";
              }
              else
              {
                if (fingerprint.Size != download.Size)
                  str = str + "\r\nSize mismatch (" + fingerprint.Size + " vs " + download.Size + ")";
                if (fingerprint.Checksum != download.Checksum)
                  str = str + "\r\nChecksum Mismatch (" + fingerprint.Checksum + " vs " + download.Checksum + ")";
              }
              if (download.Warn)
                this.m_ErrorLog.Add(str);
              else
                this.m_WarningLog.Add(str);
            }
            else
              MyToolkit.ActivityLog("Download failed, trying from a different URL");
          }
          else
          {
            if (File.Exists(download.FullName))
            {
              File.SetAttributes(download.FullName, File.GetAttributes(download.FullName) & ~FileAttributes.ReadOnly);
              File.Delete(download.FullName);
            }
            flag = false;
            File.Move(download.FullName + ".download", download.FullName);
            this.FlagVerified(download.FullName, download.Size, download.Checksum);
          }
        }
        this.m_Downloaded += download.Size;
      }
      this.m_Status = "Done";
      this.m_current = "";
    }

    private void DownloadFileComplete(object sender, AsyncCompletedEventArgs e)
    {
      this.m_current = "";
      this.m_DownloadActive = false;
    }

    private void dlProgress(object sender, DownloadProgressChangedEventArgs e)
    {
      this.m_CurrDownloadBytes = e.BytesReceived;
      this.m_progress = (int) Math.Round((this.m_CurrDownloadBytes + this.m_Downloaded) / (double) this.m_DownloadSize * 100.0, 0);
    }

    public void DownloadManifest()
    {
      if (WorkThread.DontDownloadManifest)
        this.ManifestDownloadComplete(null, null);
      MyToolkit.ActivityLog("Attempting to download Manifest file \"" + this.ManifestURL + "\"");
      this.m_Status = "Fetching manifest";
      this.LocalManifest = MyToolkit.ValidPath(Path.Combine(this.PathRoot, "CreamSoda.xml"));
      this.client.StartDownload(new AsyncCompletedEventHandler(this.ManifestDownloadComplete), new DownloadProgressChangedEventHandler(this.dlProgress), this.ManifestURL, this.LocalManifest);
    }

    private void ManifestDownloadComplete(object sender, AsyncCompletedEventArgs e)
    {
      if (e != null && e.Error != null)
      {
        MyToolkit.ActivityLog("Manifest download error for " + this.ManifestURL + "\r\n" + e.Error.Message);
        this.m_ErrorLog.Add("Manifest download error for " + this.ManifestURL + "\r\n" + e.Error.Message);
        this.m_Status = "Done";
      }
      else if (!File.Exists(this.LocalManifest))
      {
        MyToolkit.ActivityLog("Error downloading manifest, download complete but no file found locally.");
        this.m_ErrorLog.Add("Error downloading manifest");
        this.m_Status = "Done";
      }
      else
      {
        FileInfo fileInfo = new FileInfo(this.LocalManifest);
        if (fileInfo.Length != this.client.Length)
        {
          MyToolkit.ActivityLog("Error downloading manifest, downloaded file not the right size. Expected: " + fileInfo.Length + " received: " + client.Length);
          this.m_ErrorLog.Add("Error downloading manifest");
          this.m_Status = "Done";
        }
        else
        {
          this.m_current = "";
          MyToolkit.ActivityLog("Manifest downloaded successfully, starting to process it.");
          this.m_ManifestFileList = new ArrayList();
          try
          {
            this.m_manifest = XElement.Load(this.LocalManifest);
            using (IEnumerator<XElement> enumerator = this.m_manifest.Descendants("webpage").GetEnumerator())
            {
              if (enumerator.MoveNext())
                this.ForumURL = enumerator.Current.Value;
            }
            this.SelfPatch();
            this.m_Status = "Reading manifest";
            foreach (XElement descendant1 in this.m_manifest.Descendants("file"))
            {
              if (WorkThread.Kill)
                return;
              long result;
              long.TryParse(descendant1.Attribute("size").Value.ToString(), out result);
              bool flag = true;
              if (descendant1.Attribute("warn") != null && descendant1.Attribute("warn").Value == "no")
                flag = false;
              string Checksum = descendant1.Attribute("md5").Value;
              string FileName = descendant1.Attribute("name").Value;
              if (FileName.Trim() != "")
              {
                                Fingerprint fingerprint = new Fingerprint(this.PathRoot, FileName, Checksum, result)
                                {
                                    Warn = flag
                                };
                                foreach (XElement descendant2 in descendant1.Descendants("url"))
                  fingerprint.AddDownloadURL(descendant2.Value.ToString().Trim());
                this.m_ManifestFileList.Add(fingerprint);
              }
            }
            this.m_progress = 0;
            this.m_Status = "Verifying";
            this.myWorkThread = new Thread(new ThreadStart(this.Validate));
            this.myWorkThread.Start();
          }
          catch (Exception ex)
          {
            int num = (int) MessageBox.Show(ex.Message, "WorkThread.ManifestDownloadComplete()");
            string message = ex.Message;
          }
        }
      }
    }

    public void Cancel()
    {
      if (this.myWorkThread == null || !this.myWorkThread.IsAlive)
        return;
      MyToolkit.ActivityLog("Patching process canceled.");
      try
      {
        this.myWorkThread.Abort();
      }
      catch (Exception)
            {
      }
    }

    private void SelfPatch()
    {
      try
      {
        Fingerprint fingerprint1 = new Fingerprint(Settings.GamePath, "CreamSoda.exe");
        if (WorkThread.DontSelfUpdate)
          return;
        MyToolkit.ActivityLog("Starting self-patch process.");
        foreach (string file in Directory.GetFiles(Settings.GamePath, "*.old"))
        {
          try
          {
            File.Delete(file);
          }
          catch (Exception)
                    {
          }
        }
        IEnumerable<XElement> xelements = this.m_manifest.Descendants("launcher");
        this.m_Status = "Self patching";
        foreach (XElement xelement in xelements)
        {
          if (xelement.Attribute("id").Value == "CreamSoda")
          {
            long result = 0;
            long.TryParse(xelement.Attribute("size").Value.ToString(), out result);
            Fingerprint other = new Fingerprint(Settings.GamePath, "CreamSoda.exe", xelement.Attribute("md5").Value, result);
            if (!fingerprint1.Equals(other))
            {
              MyToolkit.ActivityLog("Patcher out of date...");
              foreach (XElement descendant in xelement.Descendants("url"))
                other.AddDownloadURL(descendant.Value);
              HTTP http = new HTTP();
              this.m_DownloadSize = other.Size;
              string downloadUrl = other.DownloadURL;
              MyToolkit.ActivityLog("Downloading new version from \"" + downloadUrl + "\"");
              if (http.StartDownload(new AsyncCompletedEventHandler(this.DownloadFileComplete), new DownloadProgressChangedEventHandler(this.dlProgress), downloadUrl, other.FullName + ".download"))
              {
                this.m_Status = "Downloading";
                this.m_DownloadActive = true;
              }
              this.m_current = other.FullName;
              MyToolkit.ActivityLog("Waiting for patcher download to complete...");
              while (http.Active)
              {
                if (WorkThread.Kill)
                {
                  http.CancelDownload();
                  return;
                }
                Thread.Sleep(10);
              }
              this.m_DownloadActive = false;
              MyToolkit.ActivityLog("New patcher version downloaded...");
              Fingerprint fingerprint2 = new Fingerprint(other.RootPath, other.FileName + ".download");
              if (!fingerprint2.Equals(other))
              {
                string Line = "Was unable to self patch. Downloaded file did not match expected checksum." + "\r\n" + other.FileName + "\r\n md5: " + other.Checksum + " vs " + fingerprint2.Checksum + "\r\n size: " + other.Size + " vs " + fingerprint2.Size;
                MyToolkit.ActivityLog(Line);
                File.Delete(fingerprint2.FullName + ".download");
                this.m_ErrorLog.Add(Line);
                this.m_Status = "Done";
                return;
              }
              long num = 0;
              string str = fingerprint1.FullName + "_";
              while (File.Exists(str + num.ToString() + ".old"))
                ++num;
              string destFileName = str + num.ToString() + ".old";
              File.Move(fingerprint1.FullName, destFileName);
              File.Move(fingerprint1.FullName + ".download", fingerprint1.FullName);
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = fingerprint1.FullName,
                                Arguments = MyToolkit.AllArgs()
                            };
                            MyToolkit.ActivityLog("CreamSoda has been patched successfuly. Restarting.");
              Process.Start(startInfo);
              Application.Exit();
              return;
            }
          }
        }
        MyToolkit.ActivityLog("Self patching process complete.");
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message, "WorkThread.SelfPatch()");
      }
    }
  }
}
