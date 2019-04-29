using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CreamSoda
{
  public class CreamSoda : Form
  {
    private string ManifestURL = "";
    private WorkThread myWorker;
    private DirCopy myCopyObj;
    private Thread myCopyDirThread;
    private bool NoMove;
    private bool DevMode;
    protected bool loaded;
    private IContainer components;
    private Label lblStatus;
    private TextBox txtErrors;
    private Panel pnlErrors;
    private Label label1;
    private Button btnPlay;
    private ListBox ListBox1;
    private System.Windows.Forms.Timer timer1;
    private Button btnScreenshots;
    private Button btnOptions;
        private WebBrowser webBrowser1;
        private ProgressBar Progress;
        private ComboBox cbManifest;

    public CreamSoda()
    {
      this.InitializeComponent();
    }

    private bool Setup()
    {
      try
      {
        if (Settings.SetupNeeded)
        {
          MyToolkit.ActivityLog("Setting up CreamSoda");
          string selectedPath;
          do
          {
                        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
                        {
                            Description = "Select a location where you would like to install CreamSoda; preferably under My Documents or Application Data. Do not use a folder under Program Files.",
                            SelectedPath = Application.StartupPath
                        };
                        if (folderBrowserDialog.ShowDialog(this) == DialogResult.Cancel)
            {
              int num = (int) MessageBox.Show("You must select a valid install directory to continue.\nCreamSoda will now quit. Restart CreamSoda once you have a valid installation path.");
              Application.Exit();
              return false;
            }
            selectedPath = folderBrowserDialog.SelectedPath;
          }
          while (!true);
          Settings.GamePath = selectedPath;
          MyToolkit.ActivityLog("CreamSoda installed at \"" + selectedPath + "\"");
        }
        this.SelfRelocate();
        return true;
      }
      catch (Exception ex)
      {
        string source = this.Name + ".Setup";
        MyToolkit.ErrorReporter(ex, source);
        return false;
      }
    }

    private void SelfRelocate()
    {
      if (this.NoMove)
        return;
      Preferences.SelfRelocate();
    }

    private void Skin()
    {
      this.BackColor = Settings.BGColor;
      this.label1.ForeColor = Settings.TextColor;
      this.lblStatus.ForeColor = Settings.TextColor;
      //this.ListBox1.BackColor = Settings.BGColor;
      //this.ListBox1.ForeColor = Settings.TextColor;
    }

    private void ScanParameters()
    {
      if (MyToolkit.AllArgs().Trim() != "")
        MyToolkit.ActivityLog("Launched with following parameters: " + MyToolkit.AllArgs());
      for (int index = 0; index < MyToolkit.args.Length; ++index)
      {
        if (MyToolkit.args[index].Trim() == "-o")
          WorkThread.DontDownloadManifest = true;
        else if (MyToolkit.args[index].Trim() == "-noselfpatch" || MyToolkit.args[index].Trim() == "-noselfupdate" || MyToolkit.args[index].Trim() == "-nodisassemblejohnny5")
          WorkThread.DontSelfUpdate = true;
        else if (MyToolkit.args[index].Trim() == "-md5")
        {
          this.NoMove = true;
          WorkThread.DontSelfUpdate = true;
          Fingerprint fingerprint = new Fingerprint(Application.StartupPath, Application.ExecutablePath.Replace(Application.StartupPath + "\\", ""));
          Clipboard.SetText("md5=\"" + fingerprint.Checksum + "\" size=\"" + fingerprint.Size + "\"");
        }
        else if (MyToolkit.args[index].Trim() == "-nomove")
        {
          this.NoMove = true;
          WorkThread.DontSelfUpdate = true;
        }
        else if (MyToolkit.args[index].Trim() == "-devmode" || MyToolkit.args[index].Trim() == "-dev")
          this.DevMode = true;
        else if (MyToolkit.args.Length > index + 1 && MyToolkit.args[index].Trim() == "-m")
        {
          if (MyToolkit.args[index + 1].Trim() == "")
          {
            int num = (int) MessageBox.Show("No manifest specified in parameter -m, using default.");
          }
          else
          {
            this.ManifestURL = MyToolkit.args[index + 1];
            List<string> manifests = Settings.Manifests;
            bool flag = false;
            foreach (string str in manifests)
            {
              if (str.Equals(this.ManifestURL, StringComparison.CurrentCultureIgnoreCase))
              {
                flag = true;
                break;
              }
            }
            if (!flag)
            {
              manifests.Add(this.ManifestURL);
              Settings.Manifests = manifests;
            }
            Settings.LastManifest = this.ManifestURL;
          }
        }
      }
    }

    private void ProcessKiller()
    {
      try
      {
        Process[] processesByName = Process.GetProcessesByName("CreamSoda");
        Process currentProcess = Process.GetCurrentProcess();
        int num1 = 0;
        int num2 = 0;
        foreach (Process process in processesByName)
        {
          if (process.Id != currentProcess.Id)
          {
            MyToolkit.ActivityLog("Shutting down previous instance of patcher...");
            ++num1;
            try
            {
              process.Kill();
            }
            catch (Exception)
                        {
              ++num2;
            }
          }
        }
        if (num1 > 0)
          Thread.Sleep(2000);
        if (num2 <= 0)
          return;
        int num3 = (int) MessageBox.Show(null, "Found a running instance of CreamSoda but was not able to terminate it.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      catch (Exception)
            {
      }
    }

    private void Form_Load(object sender, EventArgs e)
    {
      MyToolkit.ActivityLog("Loading application.");
      try
      {
        this.ProcessKiller();
        this.Text = this.Text + " " + Application.ProductVersion;
        this.Skin();
        this.ScanParameters();
        this.LoadManifestList();
        this.timer1.Enabled = this.Setup();
        this.ListBox1.ClearSelected();
      } catch (Exception ex)
      {
        string source = this.Name + ".Form_Load";
        MyToolkit.ErrorReporter(ex, source);
      }
      this.loaded = true;
      MyToolkit.ActivityLog("Load application complete.");
    }

    public void LoadManifestList()
    {
      this.loaded = false;
      List<string> manifests = Settings.Manifests;
      if (manifests.Count == 0)
      {
        manifests.Add("http://thunderspy.com/manifest.xml");
        Settings.Manifests = manifests;
      }
      this.cbManifest.DataSource = manifests;
      this.ManifestURL = manifests[0];
      for (int index = 0; index < manifests.Count; ++index)
      {
        if (manifests[index] == Settings.LastManifest)
        {
          this.cbManifest.SelectedIndex = index;
          this.ManifestURL = Settings.LastManifest;
          break;
        }
      }
      this.loaded = true;
    }

    private void StartUp()
    {
      try
      {
        MyToolkit.ActivityLog("Started patching");
        string gamePath = Settings.GamePath;
        string str = gamePath + "CreamSoda.xml";
        this.btnPlay.Text = "Please wait...";
        this.btnPlay.Enabled = false;
        this.cbManifest.Enabled = false;
                this.myWorker = new WorkThread(this.ManifestURL)
                {
                    LocalManifest = str,
                    PathRoot = gamePath
                };
                this.myWorker.DownloadManifest();
      }
      catch (Exception ex)
      {
        string source = this.Name + ".StartUp";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void Finish()
    {
      try
      {
        MyToolkit.ActivityLog("Finished patching.");
        this.Progress.Value = 100;
        this.timer1.Enabled = false;
        if (this.myWorker.ErrorMessage != "")
        {
          this.txtErrors.Text = this.myWorker.ErrorMessage;
          this.webBrowser1.Visible = false;
          this.pnlErrors.Visible = true;
        }
        else
        {
          this.btnPlay.Enabled = true;
          this.cbManifest.Enabled = true;
          this.btnPlay.Text = "Play";
        }
      }
      catch (Exception ex)
      {
        string source = this.Name + ".Finish";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      try
      {
        if (this.myCopyObj != null && this.myCopyObj.Active)
          this.Progress.Value = this.myCopyObj.Progress;
        else if (this.myWorker == null)
        {
          this.StartUp();
        }
        else
        {
          if (this.myWorker.ForumURL != "" && (object) this.myWorker.ForumURL != this.webBrowser1.Tag && (this.myWorker.ForumURL != this.webBrowser1.Url.AbsoluteUri && !this.webBrowser1.IsBusy))
          {
            MyToolkit.ActivityLog("Loading Web Browser URL to: \"" + this.myWorker.ForumURL + "\"");
            this.webBrowser1.Tag = myWorker.ForumURL;
            this.webBrowser1.Navigate(this.myWorker.ForumURL);
          }
          if (this.myWorker.Manifest != null && this.ListBox1.Items.Count <= 1)
          {
            IEnumerable<XElement> xelements = this.myWorker.Manifest.Descendants("launch");
            List<object> objectList = new List<object>();
            foreach (XElement xelement in xelements)
              objectList.Add(new LaunchProfile(xelement.Value.ToString().Replace("My App: ", "").Trim(), xelement.Attribute("exec").Value, xelement.Attribute("website").Value, xelement.Attribute("params").Value));
            if (this.DevMode)
            {
              foreach (XElement descendant in this.myWorker.Manifest.Descendants("devlaunch"))
                objectList.Add(new LaunchProfile(descendant.Value.ToString().Replace("My App: ", "").Trim(), descendant.Attribute("exec").Value, descendant.Attribute("website").Value, descendant.Attribute("params").Value));
            }
            this.ListBox1.DisplayMember = "Text";
            this.ListBox1.DataSource = objectList;
            this.ListBox1.SelectedIndex = 0;
          }
          this.Progress.Value = MyToolkit.MinMax(this.myWorker.CurProgress, 0, 100);
          this.lblStatus.Text = this.myWorker.Status + "... " + this.myWorker.CurFile;
          if (!(this.myWorker.Status == "Done"))
            return;
          this.Finish();
        }
      }
      catch (Exception ex)
      {
        string source = this.Name + ".Form_Load";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void btnPlay_Click(object sender, EventArgs e)
    {
      try
      {
        MyToolkit.ActivityLog("User clicked play with the following profile: " + ((LaunchProfile) this.ListBox1.SelectedItem).Text);
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
          WorkingDirectory = Settings.GamePath,
          FileName = ((LaunchProfile) this.ListBox1.SelectedItem).Exec,
          Arguments = ((LaunchProfile) this.ListBox1.SelectedItem).Params
        };
        startInfo.Arguments = startInfo.Arguments + " " + Settings.GameParams;
        Process.Start(startInfo);
        if (!Settings.QuitOnLaunch)
          return;
        Application.Exit();
      }
      catch (Exception ex)
      {
        string source = this.Name + ".btnPlay_Click";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void btnScreenshots_Click(object sender, EventArgs e)
    {
      try
      {
        MyToolkit.ActivityLog("User clicked to open screenshots directory");
        string str = Path.Combine(Settings.GamePath, "screenshots");
        if (!Directory.Exists(str))
          Directory.CreateDirectory(str);
        Process.Start("explorer.exe", str);
      }
      catch (Exception ex)
      {
        string source = this.Name + ".btScreenshots_Click";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void Form_FormClosing(object sender, FormClosingEventArgs e)
    {
      MyToolkit.ActivityLog("Application quitting");
      WorkThread.Kill = true;
      DirCopy.Kill = true;
      if (this.myCopyDirThread == null)
        return;
      if (!this.myCopyDirThread.IsAlive)
        return;
      try
      {
        this.myCopyDirThread.Abort();
      }
      catch (Exception)
            {
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      MyToolkit.ActivityLog("User clicked to open Options window");
      Preferences preferences = new Preferences();
      preferences.btnRevalidate.Enabled = this.myWorker.Status == "Done";
      int num = (int) preferences.ShowDialog(this);
      this.Skin();
      this.LoadManifestList();
      if (!preferences.ReValidate)
        return;
      this.ReValidate();
    }

    private void ReValidate()
    {
      try
      {
        this.pnlErrors.Visible = false;
        this.webBrowser1.Visible = true;
        MyToolkit.ActivityLog("Revalidation process started");
        this.ListBox1.DataSource = null;
        File.Delete(Path.Combine(Settings.GamePath, "CreamSodalog.xml"));
        this.timer1.Enabled = this.Setup();
        this.StartUp();
      }
      catch (Exception ex)
      {
        string source = this.Name + ".Form_Load";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void cbManifest_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!this.loaded || !(Settings.LastManifest != this.cbManifest.SelectedItem.ToString()))
        return;
      MyToolkit.ActivityLog("Manifest changed to \"" + this.cbManifest.SelectedItem.ToString() + "\"");
      Settings.LastManifest = this.cbManifest.SelectedItem.ToString();
      this.ManifestURL = Settings.LastManifest;
      this.ReValidate();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtErrors = new System.Windows.Forms.TextBox();
            this.pnlErrors = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPlay = new System.Windows.Forms.Button();
            this.ListBox1 = new System.Windows.Forms.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnScreenshots = new System.Windows.Forms.Button();
            this.btnOptions = new System.Windows.Forms.Button();
            this.cbManifest = new System.Windows.Forms.ComboBox();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.pnlErrors.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(784, 279);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BackColor = System.Drawing.Color.Transparent;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblStatus.Location = new System.Drawing.Point(7, 471);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(765, 21);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Starting...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtErrors
            // 
            this.txtErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtErrors.Location = new System.Drawing.Point(0, 0);
            this.txtErrors.Margin = new System.Windows.Forms.Padding(0);
            this.txtErrors.Multiline = true;
            this.txtErrors.Name = "txtErrors";
            this.txtErrors.Size = new System.Drawing.Size(759, 267);
            this.txtErrors.TabIndex = 2;
            this.txtErrors.Visible = false;
            // 
            // pnlErrors
            // 
            this.pnlErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlErrors.BackColor = System.Drawing.Color.Transparent;
            this.pnlErrors.Controls.Add(this.label1);
            this.pnlErrors.Controls.Add(this.txtErrors);
            this.pnlErrors.Location = new System.Drawing.Point(13, 12);
            this.pnlErrors.Name = "pnlErrors";
            this.pnlErrors.Size = new System.Drawing.Size(759, 267);
            this.pnlErrors.TabIndex = 3;
            this.pnlErrors.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 29);
            this.label1.TabIndex = 3;
            this.label1.Text = "Errors:";
            // 
            // btnPlay
            // 
            this.btnPlay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlay.Enabled = false;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlay.Location = new System.Drawing.Point(12, 401);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(373, 40);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // ListBox1
            // 
            this.ListBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListBox1.BackColor = System.Drawing.SystemColors.Window;
            this.ListBox1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListBox1.ForeColor = System.Drawing.SystemColors.InfoText;
            this.ListBox1.FormattingEnabled = true;
            this.ListBox1.ItemHeight = 25;
            this.ListBox1.Location = new System.Drawing.Point(13, 316);
            this.ListBox1.Name = "ListBox1";
            this.ListBox1.Size = new System.Drawing.Size(759, 79);
            this.ListBox1.TabIndex = 0;
            this.ListBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListBox1_MouseClick);
            this.ListBox1.DoubleClick += new System.EventHandler(this.btnPlay_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // btnScreenshots
            // 
            this.btnScreenshots.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScreenshots.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.btnScreenshots.Location = new System.Drawing.Point(392, 401);
            this.btnScreenshots.Name = "btnScreenshots";
            this.btnScreenshots.Size = new System.Drawing.Size(188, 40);
            this.btnScreenshots.TabIndex = 3;
            this.btnScreenshots.Text = "Screenshots";
            this.btnScreenshots.UseVisualStyleBackColor = true;
            this.btnScreenshots.Click += new System.EventHandler(this.btnScreenshots_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.btnOptions.Location = new System.Drawing.Point(586, 401);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(186, 40);
            this.btnOptions.TabIndex = 4;
            this.btnOptions.Text = "Options";
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbManifest
            // 
            this.cbManifest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbManifest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbManifest.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbManifest.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbManifest.FormattingEnabled = true;
            this.cbManifest.Location = new System.Drawing.Point(13, 285);
            this.cbManifest.Name = "cbManifest";
            this.cbManifest.Size = new System.Drawing.Size(759, 25);
            this.cbManifest.TabIndex = 2;
            this.cbManifest.SelectedIndexChanged += new System.EventHandler(this.cbManifest_SelectedIndexChanged);
            // 
            // Progress
            // 
            this.Progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Progress.Location = new System.Drawing.Point(13, 447);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(759, 21);
            this.Progress.TabIndex = 6;
            // 
            // CreamSoda
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(784, 501);
            this.Controls.Add(this.cbManifest);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.btnScreenshots);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.ListBox1);
            this.Controls.Add(this.pnlErrors);
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.webBrowser1);
            this.DoubleBuffered = true;
            this.Icon = global::CreamSoda.Resources.CreamSodaIcon;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "CreamSoda";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cream Soda";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.pnlErrors.ResumeLayout(false);
            this.pnlErrors.PerformLayout();
            this.ResumeLayout(false);

        }

        private void ListBox1_MouseClick(object sender, MouseEventArgs e)
        {
            this.webBrowser1.Navigate(((LaunchProfile)this.ListBox1.SelectedItem).Website);
        }
    }
}
