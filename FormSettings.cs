using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;
using System.IO.IsolatedStorage;


namespace SevenDeskBar
{
    public partial class FormSettings : Form
    {
        private bool OnChangingLang = false;

        public FormSettings()
        {
            LanguageManager.ChangeLanguage(this);
            InitializeComponent();
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog SelectedFolder = new FolderBrowserDialog();
            if (SelectedFolder.ShowDialog() == DialogResult.OK)
            {
                ListFolders.Items.Add(SelectedFolder.SelectedPath);
            }


        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            while (ListFolders.SelectedItems.Count != 0)
            {
                this.ListFolders.Items.Remove(ListFolders.SelectedItems[0]);
            }
            
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();

            string FolderToInclude = "";

            for (int FolderIdx = 0; FolderIdx < this.ListFolders.Items.Count; FolderIdx++)
            {
                FolderToInclude += "|" + this.ListFolders.Items[FolderIdx].ToString();
            }

            MySettings.IncludedFolders = FolderToInclude;
            MySettings.FolderCaption = this.CheckBoxCaptionFolder.Checked;
            MySettings.TollbarDockPosition = this.CombotoolbarPosition.SelectedIndex;
            MySettings.LanguageSelected = this.comboLanguage.SelectedIndex;
            MySettings.FolderActiveControl = this.checkFolderWatcher.Checked;
            MySettings.ToolbarHeightWidth = (int)this.ToolbarWidthHeight.Value;
            MySettings.LargeIcons = this.CheckboxLargeIcons.Checked;
            MySettings.AutoHide = this.CheckboxAutoHide.Checked;
            MySettings.AutoHideInterval = (int)this.NumericAutoHideInterval.Value;
            MySettings.ShowText = this.CheckboxShowText.Checked;
            MySettings.GlassStyleEffect = this.CheckboxAero.Checked;
            MySettings.ShowHidden = this.CheckShowHidden.Checked;
            MySettings.Save();
            Program.bRebootNeeded = this.CheckboxAero.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            string FoldersToAnalize = MySettings.IncludedFolders;
            string[] FolderArray = FoldersToAnalize.Split('|');
            foreach (string AttFolder in FolderArray)
            {
                if (!String.IsNullOrEmpty(AttFolder))
                {
                    this.ListFolders.Items.Add(AttFolder);
                }
            }
            this.CheckBoxCaptionFolder.Checked = MySettings.FolderCaption;
            this.CombotoolbarPosition.SelectedIndex = MySettings.TollbarDockPosition;
            this.comboLanguage.SelectedIndex = MySettings.LanguageSelected ;
            this.checkFolderWatcher.Checked = MySettings.FolderActiveControl;
            this.ToolbarWidthHeight.Value = MySettings.ToolbarHeightWidth;
            this.CheckboxLargeIcons.Checked = MySettings.LargeIcons;
            this.CheckboxAutoHide.Checked = MySettings.AutoHide;
            this.NumericAutoHideInterval.Value = MySettings.AutoHideInterval;
            this.CheckboxShowText.Checked = MySettings.ShowText;
            this.CheckboxAero.Checked = MySettings.GlassStyleEffect && VistaApi.IsGlassEnabled();
            this.CheckboxAero.Enabled = VistaApi.IsGlassEnabled();
            this.CheckShowHidden.Checked = MySettings.ShowHidden;
        }

        private void ListFolders_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                string AttPath = "";
                if (ListFolders.SelectedIndex >= 0)
                {
                    AttPath = ListFolders.Items[ListFolders.SelectedIndex].ToString();                    
                }
                FolderEdit ChangeFolderName = new FolderEdit(AttPath );
                if (ChangeFolderName.ShowDialog() == DialogResult.OK)
                {
                    if (ListFolders.SelectedIndex >= 0)
                    {
                        ListFolders.Items[ListFolders.SelectedIndex] = ChangeFolderName.GetFolderPath();
                    }
                    else
                    {
                        ListFolders.Items.Add(ChangeFolderName.GetFolderPath());
                    }
                }
            }
        }

        private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!OnChangingLang)
            {
                OnChangingLang = true;
                if (this.comboLanguage.SelectedIndex == (int)ApplLanguage.Italian)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");
                    LanguageManager.ChangeLanguage(this, "it-IT");
                }
                else if (this.comboLanguage.SelectedIndex == (int)ApplLanguage.English)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    LanguageManager.ChangeLanguage(this, "en-US");
                }
                else if (this.comboLanguage.SelectedIndex == (int)ApplLanguage.Francais)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
                    LanguageManager.ChangeLanguage(this, "fr-FR");
                }
                else if (this.comboLanguage.SelectedIndex == (int)ApplLanguage.Espanol)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
                    LanguageManager.ChangeLanguage(this, "es-ES");
                }
                else if (this.comboLanguage.SelectedIndex == (int)ApplLanguage.Deutsch)
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
                    LanguageManager.ChangeLanguage(this, "de-DE");
                }
                OnChangingLang = false;
            }
            
        }

        private void CombotoolbarPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabelWidth.Visible = CombotoolbarPosition.SelectedIndex == 0 || CombotoolbarPosition.SelectedIndex == 2;
            LabelHeight.Visible = !LabelWidth.Visible;
        }

        private void CheckboxLargeIcons_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CheckboxLargeIcons.Checked && this.ToolbarWidthHeight.Value<50)
            {
                this.ToolbarWidthHeight.Value = 50;
            }
            if (!this.CheckboxLargeIcons.Checked && this.ToolbarWidthHeight.Value == 50)
            {
                this.ToolbarWidthHeight.Value = 30;
            }
        }

        private void ButtonDeleteOrderData_Click(object sender, EventArgs e)
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetStore(IsolatedStorageScope.User |
                IsolatedStorageScope.Assembly |
                IsolatedStorageScope.Domain,
                typeof(System.Security.Policy.Url),
                typeof(System.Security.Policy.Url));

            String[] dirNames = isoFile.GetDirectoryNames("*");
            String[] fileNames = isoFile.GetFileNames("SevenDeskBar*");

            // List the files currently in this Isolated Storage.
            // The list represents all users who have personal
            // preferences stored for this application.
            if (fileNames.Length > 0)
            {
                for (int i = 0; i < fileNames.Length; ++i)
                {
                    // Delete the files.
                    isoFile.DeleteFile(fileNames[i]);
                }
                // Confirm that no files remain.
                fileNames = isoFile.GetFileNames("SevenDeskBar*");
            }
            MessageBox.Show(ResourceMessages.OrderDataDeleted, ResourceMessages.GenericInformationTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            this.MoveFolderIndex(1);
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            this.MoveFolderIndex(-1);
        }

        private void MoveFolderIndex(int Direction)
        {
            string OldValue = this.ListFolders.Text;
            int Position = this.ListFolders.SelectedIndex;
            if (Position>=0) 
            {
                this.ListFolders.Items.RemoveAt(Position);
                this.ListFolders.Items.Insert(Position + Direction, OldValue);
                this.ListFolders.SelectedIndex = Position + Direction;
            }
        }

        private void ListFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.buttonDown.Enabled = this.ListFolders.SelectedItems.Count == 1 &&
                this.ListFolders.SelectedIndex < this.ListFolders.Items.Count - 1;

            this.buttonUp.Enabled = this.ListFolders.SelectedItems.Count == 1 &&
                this.ListFolders.SelectedIndex > 0;
        }

        
    }
}
