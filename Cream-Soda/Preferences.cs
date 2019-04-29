using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CreamSoda
{
  public class Preferences : Form
  {
    public bool ReValidate;
    private IContainer components;
    private ColorDialog colorDialog1;
    private Button btnOK;
    private Button btnColor;
    private Label label1;
    private Label label2;
    private TextBox txtParameters;
    private CheckBox ckbQuitOnLaunch;
    private Button btnTextColor;
    private Label label3;
    public Button btnRevalidate;
    private ListBox lbManifests;
    private GroupBox groupBox1;
    private Button btnAddManifest;
    private TextBox txtNewManifest;
    private GroupBox groupBox2;
    private GroupBox groupBox3;
    private Button btnInstallPathBrowse;
    private Label lblInstallPath;
    private GroupBox groupBox4;
    private Button btnDelete;

    public Preferences()
    {
      this.InitializeComponent();
      this.lblInstallPath.Text = Settings.GamePath;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
      if (this.lbManifests.Text != Settings.LastManifest)
      {
        Settings.LastManifest = this.lbManifests.Text;
        this.ReValidate = true;
      }
      this.Close();
    }

    private void btnColor_Click(object sender, EventArgs e)
    {
      this.colorDialog1.Color = Settings.BGColor;
      int num = (int) this.colorDialog1.ShowDialog(this);
      this.btnColor.BackColor = this.colorDialog1.Color;
      Settings.BGColor = this.colorDialog1.Color;
    }

    private void btnTextColor_Click(object sender, EventArgs e)
    {
      this.colorDialog1.Color = Settings.TextColor;
      int num = (int) this.colorDialog1.ShowDialog(this);
      this.btnTextColor.BackColor = this.colorDialog1.Color;
      Settings.TextColor = this.colorDialog1.Color;
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      Settings.QuitOnLaunch = this.ckbQuitOnLaunch.Checked;
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      Settings.GameParams = this.txtParameters.Text.Trim();
    }

    private void Preferences_Load(object sender, EventArgs e)
    {
      this.ckbQuitOnLaunch.Checked = Settings.QuitOnLaunch;
      this.txtParameters.Text = Settings.GameParams;
      this.btnColor.BackColor = Settings.BGColor;
      this.btnTextColor.BackColor = Settings.TextColor;
      List<string> manifests = Settings.Manifests;
      this.lbManifests.DataSource = manifests;
      try
      {
        for (int index = 0; index < manifests.Count; ++index)
        {
          if (manifests[index] == Settings.LastManifest)
            this.lbManifests.SelectedIndex = index;
        }
      }
      catch (Exception)
            {
      }
    }

    private void btnRevalidate_Click(object sender, EventArgs e)
    {
      this.ReValidate = true;
      this.Close();
    }

    private void btnInstallPathBrowse_Click(object sender, EventArgs e)
    {
      string selectedPath;
      do
      {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "Select a location where you would like to install CreamSoda; preferably under My Documents or Application Data. Do not use a folder under Program Files.",
                    SelectedPath = Settings.GamePath
                };
                if (folderBrowserDialog.ShowDialog(this) == DialogResult.Cancel)
          return;
        selectedPath = folderBrowserDialog.SelectedPath;
      }
      while (!true);
      Preferences.SelfRelocate();
      Settings.GamePath = selectedPath;
    }

    private void btnDeleteManifest_Click(object sender, EventArgs e)
    {
      this.DeleteSelectedManifest();
      this.btnDelete.Enabled = false;
    }

    private void btnAddManifest_Click(object sender, EventArgs e)
    {
      List<string> dataSource = (List<string>) this.lbManifests.DataSource;
      for (int index = 0; index < dataSource.Count; ++index)
      {
        if (dataSource[index].Equals(this.txtNewManifest.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
          this.txtNewManifest.Text = "";
          this.lbManifests.SelectedIndex = index;
          return;
        }
      }
      dataSource.Add(this.txtNewManifest.Text);
      Settings.Manifests = dataSource;
      this.lbManifests.DataSource = Settings.Manifests;
      Settings.LastManifest = this.txtNewManifest.Text.Trim();
      this.txtNewManifest.Text = "";
      try
      {
        for (int index = 0; index < dataSource.Count; ++index)
        {
          if (dataSource[index] == Settings.LastManifest)
            this.lbManifests.SelectedIndex = index;
        }
      }
      catch (Exception)
            {
      }
    }

    private void DeleteSelectedManifest()
    {
      List<string> dataSource = (List<string>) this.lbManifests.DataSource;
      int selectedIndex = this.lbManifests.SelectedIndex;
      dataSource.RemoveAt(this.lbManifests.SelectedIndex);
      Settings.Manifests = dataSource;
      this.lbManifests.DataSource = Settings.Manifests;
      try
      {
        this.lbManifests.SelectedIndex = selectedIndex - 1;
      }
      catch (Exception)
            {
        this.lbManifests.SelectedIndex = 0;
      }
    }

    private void lbManifests_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Delete)
        return;
      this.DeleteSelectedManifest();
    }

    public static void SelfRelocate()
    {
      try
      {
        if (Application.StartupPath == Settings.GamePath || !File.Exists(Application.ExecutablePath))
          return;
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string str = Path.Combine(Settings.GamePath, "CreamSoda.exe");
        MyToolkit.ActivityLog("Self Relocating CreamSoda to \"" + str + "\"");
        if (!Directory.Exists(Settings.GamePath))
          Directory.CreateDirectory(Settings.GamePath);
        try
        {
          if (File.Exists(str))
            File.Delete(str);
          File.Move(Application.ExecutablePath, str);
        }
        catch (Exception)
                {
          File.Copy(Application.ExecutablePath, str);
          try
          {
            File.Move(Application.ExecutablePath, Path.Combine(Application.StartupPath, "deleteme.txt"));
          }
          catch (Exception)
                    {
            MyToolkit.ActivityLog("Failed to relocate CreamSoda to \"" + str + "\"");
          }
        }
        try
        {
          using (ShellLink shellLink = new ShellLink())
          {
            shellLink.Target = str;
            shellLink.Description = "Drink up!";
            shellLink.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
            shellLink.Save(Path.Combine(folderPath, "CreamSoda.lnk"));
          }
        }
        catch (Exception ex)
        {
          MyToolkit.ActivityLog("Failed to create desktop shortcut \"" + str + "\"");
          int num = (int) MessageBox.Show(ex.Message);
        }
      }
      catch (Exception ex)
      {
        string source = "Preferences.SelfRelocate";
        MyToolkit.ErrorReporter(ex, source);
      }
    }

    private void lbManifests_Click(object sender, EventArgs e)
    {
      this.btnDelete.Enabled = true;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Preferences));
      this.colorDialog1 = new ColorDialog();
      this.btnOK = new Button();
      this.btnColor = new Button();
      this.label1 = new Label();
      this.label2 = new Label();
      this.txtParameters = new TextBox();
      this.ckbQuitOnLaunch = new CheckBox();
      this.btnTextColor = new Button();
      this.label3 = new Label();
      this.btnRevalidate = new Button();
      this.lbManifests = new ListBox();
      this.groupBox1 = new GroupBox();
      this.btnDelete = new Button();
      this.btnAddManifest = new Button();
      this.txtNewManifest = new TextBox();
      this.groupBox2 = new GroupBox();
      this.groupBox3 = new GroupBox();
      this.btnInstallPathBrowse = new Button();
      this.lblInstallPath = new Label();
      this.groupBox4 = new GroupBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      this.colorDialog1.Color = Color.FromArgb(57, 94, 112);
      this.btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btnOK.DialogResult = DialogResult.Cancel;
      this.btnOK.Location = new Point(248, 322);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new Size(131, 33);
      this.btnOK.TabIndex = 17;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new EventHandler(this.btnOK_Click);
      this.btnColor.BackColor = Color.FromArgb(byte.MaxValue, byte.MaxValue, 128);
      this.btnColor.Location = new Point(83, 11);
      this.btnColor.Name = "btnColor";
      this.btnColor.Size = new Size(67, 23);
      this.btnColor.TabIndex = 9;
      this.btnColor.UseVisualStyleBackColor = false;
      this.btnColor.Click += new EventHandler(this.btnColor_Click);
      this.label1.AutoSize = true;
      this.label1.Location = new Point(12, 16);
      this.label1.Name = "label1";
      this.label1.Size = new Size(65, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Background";
      this.label2.AutoSize = true;
      this.label2.Location = new Point(6, 16);
      this.label2.Name = "label2";
      this.label2.Size = new Size(99, 13);
      this.label2.TabIndex = 1;
      this.label2.Text = "Launch Parameters";
      this.txtParameters.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.txtParameters.Location = new Point(111, 13);
      this.txtParameters.Name = "txtParameters";
      this.txtParameters.Size = new Size(byte.MaxValue, 20);
      this.txtParameters.TabIndex = 2;
      this.txtParameters.TextChanged += new EventHandler(this.textBox1_TextChanged);
      this.ckbQuitOnLaunch.AutoSize = true;
      this.ckbQuitOnLaunch.Location = new Point(113, 41);
      this.ckbQuitOnLaunch.Name = "ckbQuitOnLaunch";
      this.ckbQuitOnLaunch.Size = new Size(198, 17);
      this.ckbQuitOnLaunch.TabIndex = 3;
      this.ckbQuitOnLaunch.Text = "Close CreamSoda after starting the game";
      this.ckbQuitOnLaunch.UseVisualStyleBackColor = true;
      this.ckbQuitOnLaunch.CheckedChanged += new EventHandler(this.checkBox1_CheckedChanged);
      this.btnTextColor.BackColor = Color.FromArgb(byte.MaxValue, byte.MaxValue, 128);
      this.btnTextColor.Location = new Point(190, 11);
      this.btnTextColor.Name = "btnTextColor";
      this.btnTextColor.Size = new Size(67, 23);
      this.btnTextColor.TabIndex = 11;
      this.btnTextColor.UseVisualStyleBackColor = false;
      this.btnTextColor.Click += new EventHandler(this.btnTextColor_Click);
      this.label3.AutoSize = true;
      this.label3.Location = new Point(156, 16);
      this.label3.Name = "label3";
      this.label3.Size = new Size(28, 13);
      this.label3.TabIndex = 10;
      this.label3.Text = "Text";
      this.btnRevalidate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btnRevalidate.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Pixel, 0);
      this.btnRevalidate.Location = new Point(116, 322);
      this.btnRevalidate.Name = "btnRevalidate";
      this.btnRevalidate.Size = new Size(129, 33);
      this.btnRevalidate.TabIndex = 16;
      this.btnRevalidate.Text = "Re-Validate";
      this.btnRevalidate.UseVisualStyleBackColor = true;
      this.btnRevalidate.Click += new EventHandler(this.btnRevalidate_Click);
      this.lbManifests.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.lbManifests.FormattingEnabled = true;
      this.lbManifests.Location = new Point(6, 46);
      this.lbManifests.Name = "lbManifests";
      this.lbManifests.Size = new Size(304, 82);
      this.lbManifests.TabIndex = 15;
      this.lbManifests.Click += new EventHandler(this.lbManifests_Click);
      this.lbManifests.KeyUp += new KeyEventHandler(this.lbManifests_KeyUp);
      this.groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBox1.Controls.Add(btnDelete);
      this.groupBox1.Controls.Add(btnAddManifest);
      this.groupBox1.Controls.Add(txtNewManifest);
      this.groupBox1.Controls.Add(lbManifests);
      this.groupBox1.Location = new Point(7, 178);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(372, 138);
      this.groupBox1.TabIndex = 12;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Manifests";
      this.btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.btnDelete.Location = new Point(316, 46);
      this.btnDelete.Name = "btnDelete";
      this.btnDelete.Size = new Size(50, 82);
      this.btnDelete.TabIndex = 14;
      this.btnDelete.Text = "Delete";
      this.btnDelete.UseVisualStyleBackColor = true;
      this.btnDelete.Click += new EventHandler(this.btnDeleteManifest_Click);
      this.btnAddManifest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.btnAddManifest.Location = new Point(316, 16);
      this.btnAddManifest.Name = "btnAddManifest";
      this.btnAddManifest.Size = new Size(50, 23);
      this.btnAddManifest.TabIndex = 14;
      this.btnAddManifest.Text = "Add";
      this.btnAddManifest.UseVisualStyleBackColor = true;
      this.btnAddManifest.Click += new EventHandler(this.btnAddManifest_Click);
      this.txtNewManifest.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.txtNewManifest.Location = new Point(6, 18);
      this.txtNewManifest.Name = "txtNewManifest";
      this.txtNewManifest.Size = new Size(304, 20);
      this.txtNewManifest.TabIndex = 13;
      this.groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBox2.Controls.Add(btnColor);
      this.groupBox2.Controls.Add(label1);
      this.groupBox2.Controls.Add(btnTextColor);
      this.groupBox2.Controls.Add(label3);
      this.groupBox2.Location = new Point(7, 130);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(372, 42);
      this.groupBox2.TabIndex = 7;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Colors";
      this.groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBox3.Controls.Add(btnInstallPathBrowse);
      this.groupBox3.Controls.Add(lblInstallPath);
      this.groupBox3.Location = new Point(9, 78);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(370, 46);
      this.groupBox3.TabIndex = 4;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Install Path";
      this.btnInstallPathBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.btnInstallPathBrowse.Location = new Point(314, 15);
      this.btnInstallPathBrowse.Name = "btnInstallPathBrowse";
      this.btnInstallPathBrowse.Size = new Size(50, 23);
      this.btnInstallPathBrowse.TabIndex = 6;
      this.btnInstallPathBrowse.Text = "Browse";
      this.btnInstallPathBrowse.UseVisualStyleBackColor = true;
      this.btnInstallPathBrowse.Click += new EventHandler(this.btnInstallPathBrowse_Click);
      this.lblInstallPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.lblInstallPath.Location = new Point(6, 19);
      this.lblInstallPath.Name = "lblInstallPath";
      this.lblInstallPath.Size = new Size(302, 21);
      this.lblInstallPath.TabIndex = 5;
      this.lblInstallPath.Text = "label4";
      this.groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBox4.Controls.Add(label2);
      this.groupBox4.Controls.Add(txtParameters);
      this.groupBox4.Controls.Add(ckbQuitOnLaunch);
      this.groupBox4.Location = new Point(9, 9);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new Size(372, 63);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Settings";
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = btnOK;
      this.ClientSize = new Size(384, 362);
      this.ControlBox = false;
      this.Controls.Add(groupBox4);
      this.Controls.Add(groupBox3);
      this.Controls.Add(groupBox2);
      this.Controls.Add(groupBox1);
      this.Controls.Add(btnRevalidate);
      this.Controls.Add(btnOK);
      this.Icon = global::CreamSoda.Resources.CreamSodaIcon;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new Size(400, 400);
      this.Name = "Preferences";
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Options";
      this.Load += new EventHandler(this.Preferences_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.ResumeLayout(false);
    }
  }
}
