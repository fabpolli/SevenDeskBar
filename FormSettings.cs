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

        // carattere e colore scelti (null / Empty = predefiniti)
        private Font _SelectedFont;
        private Color _SelectedColor = Color.Empty;
        // opzioni specifiche delle cartelle, per percorso
        private Dictionary<string, FolderOptions> _FolderOptions = new Dictionary<string, FolderOptions>(StringComparer.OrdinalIgnoreCase);
        // valori non ancora salvati da riproporre dopo un cambio di lingua
        private SettingsDraft _InitialDraft;

        /// <summary>
        /// Valori della finestra non ancora salvati. Quando l'utente cambia lingua la
        /// finestra viene chiusa con DialogResult.Retry e ricreata nella nuova lingua con
        /// questi valori: così il layout specifico della lingua e la scalatura per lo
        /// zoom (DPI) vengono applicati da InitializeComponent come al primo avvio.
        /// (Prima la lingua veniva cambiata "al volo" riapplicando le risorse, che però
        /// riportavano posizioni e dimensioni ai valori per lo zoom al 100%.)
        /// </summary>
        public class SettingsDraft
        {
            public List<string> Folders = new List<string>();
            public Dictionary<string, FolderOptions> FolderOptions = new Dictionary<string, FolderOptions>(StringComparer.OrdinalIgnoreCase);
            public bool FolderCaption;
            public int DockPosition;
            public int Language;
            public bool FolderWatcher;
            public decimal ToolbarSize;
            public bool LargeIcons;
            public bool AutoHide;
            public decimal AutoHideInterval;
            public bool ShowText;
            public bool Aero;
            public bool ShowHidden;
            public Font TextFont;
            public Color TextColor;
            public int OverflowMode;
            public decimal OverflowColumns;
        }

        public SettingsDraft Draft;

        public FormSettings() : this(null)
        {
        }

        public FormSettings(SettingsDraft InitialDraft)
        {
            if (InitialDraft == null)
            {
                //lingua salvata nelle impostazioni
                LanguageManager.ChangeLanguage(this);
            }
            //altrimenti la finestra viene ricreata dopo un cambio di lingua: la lingua
            //scelta (non ancora salvata) è già impostata sul thread e va mantenuta
            InitializeComponent();
            //font esplicito dopo InitializeComponent: riscala la finestra allo zoom (vedi Program.DialogFont)
            this.Font = Program.DialogFont();
            this._InitialDraft = InitialDraft;
            this.UpdateExtraTexts();
        }

        private static string Msg(string Key)
        {
            return ResourceMessages.ResourceManager.GetString(Key) ?? Key;
        }

        /// <summary>Testi dei controlli aggiunti (non presenti nei file .resx)</summary>
        private void UpdateExtraTexts()
        {
            this.ButtonFolderOptions.Text = Msg("SettingsFolderOptionsButton");
            this.ButtonFont.Text = Msg("SettingsFontButton");
            this.ButtonColor.Text = Msg("SettingsColorButton");
            this.ButtonFontReset.Text = Msg("SettingsFontReset");
            this.LabelOverflow.Text = Msg("SettingsOverflowLabel");
            this.LabelOverflowColumns.Text = Msg("SettingsOverflowColumns");
            int SelIdx = this.ComboOverflow.SelectedIndex;
            this.ComboOverflow.Items.Clear();
            this.ComboOverflow.Items.Add(Msg("SettingsOverflowPopup"));
            this.ComboOverflow.Items.Add(Msg("SettingsOverflowScroll"));
            this.ComboOverflow.SelectedIndex = SelIdx >= 0 ? SelIdx : 0;
            this.UpdateFontPreview();
        }

        private Font DefaultToolbarFont()
        {
            //stesso carattere predefinito della toolbar (MainForm.resx)
            return new Font("Arial", 9F);
        }

        private void UpdateFontPreview()
        {
            Font PreviewFont = this._SelectedFont ?? this.DefaultToolbarFont();
            this.LabelFontPreview.Font = PreviewFont;
            this.LabelFontPreview.ForeColor = this._SelectedColor.IsEmpty ? SystemColors.ControlText : this._SelectedColor;
            this.LabelFontPreview.Text = Msg("SettingsFontPreview") + " - " + PreviewFont.Name + " " + PreviewFont.SizeInPoints.ToString("0.#") + "pt";
        }

        private void ButtonFont_Click(object sender, EventArgs e)
        {
            using (FontDialog Dlg = new FontDialog())
            {
                Dlg.Font = this._SelectedFont ?? this.DefaultToolbarFont();
                Dlg.ShowColor = true;
                Dlg.Color = this._SelectedColor.IsEmpty ? SystemColors.ControlText : this._SelectedColor;
                Dlg.FontMustExist = true;
                if (Dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this._SelectedFont = Dlg.Font;
                    if (Dlg.Color.ToArgb() != SystemColors.ControlText.ToArgb() || !this._SelectedColor.IsEmpty)
                    {
                        this._SelectedColor = Dlg.Color;
                    }
                    this.UpdateFontPreview();
                }
            }
        }

        private void ButtonColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog Dlg = new ColorDialog())
            {
                Dlg.FullOpen = true;
                Dlg.AnyColor = true;
                Dlg.Color = this._SelectedColor.IsEmpty ? SystemColors.ControlText : this._SelectedColor;
                if (Dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this._SelectedColor = Dlg.Color;
                    this.UpdateFontPreview();
                }
            }
        }

        private void ButtonFontReset_Click(object sender, EventArgs e)
        {
            this._SelectedFont = null;
            this._SelectedColor = Color.Empty;
            this.UpdateFontPreview();
        }

        private void ComboOverflow_SelectedIndexChanged(object sender, EventArgs e)
        {
            //il numero di colonne riguarda solo l'elenco a comparsa
            this.NumericOverflowColumns.Enabled = this.ComboOverflow.SelectedIndex == 0;
            this.LabelOverflowColumns.Enabled = this.NumericOverflowColumns.Enabled;
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
            MySettings.Reload();
            bool OldGlass = MySettings.GlassStyleEffect;

            string FolderToInclude = "";
            List<FolderOptions> OptionsToSave = new List<FolderOptions>();

            for (int FolderIdx = 0; FolderIdx < this.ListFolders.Items.Count; FolderIdx++)
            {
                string AttFolder = this.ListFolders.Items[FolderIdx].ToString();
                FolderToInclude += "|" + AttFolder;
                FolderOptions Opt;
                if (this._FolderOptions.TryGetValue(AttFolder, out Opt))
                {
                    OptionsToSave.Add(Opt);
                }
            }

            MySettings.IncludedFolders = FolderToInclude;
            MySettings.FolderOptions = FolderOptions.Serialize(OptionsToSave);
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
            MySettings.TextFont = SettingsHelper.FontToString(this._SelectedFont);
            MySettings.TextColor = SettingsHelper.ColorToString(this._SelectedColor);
            MySettings.OverflowMode = Math.Max(0, this.ComboOverflow.SelectedIndex);
            MySettings.OverflowColumns = (int)this.NumericOverflowColumns.Value;
            MySettings.Save();
            //Il riavvio della barra serve solo se l'effetto Aero è stato attivato o
            //disattivato (prima avveniva ad ogni salvataggio con Aero attivo: ticket #17)
            Program.bRebootNeeded = this.CheckboxAero.Checked != OldGlass;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            //durante il caricamento i cambi di selezione non devono ricreare la finestra
            OnChangingLang = true;
            try
            {
                if (this._InitialDraft != null)
                {
                    this.LoadFromDraft(this._InitialDraft);
                }
                else
                {
                    this.LoadFromSettings();
                }
                this.CheckboxAero.Enabled = VistaApi.IsGlassEnabled();
                this.UpdateFontPreview();
            }
            finally
            {
                OnChangingLang = false;
            }
        }

        private void LoadFromSettings()
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
            this._FolderOptions = FolderOptions.Parse(MySettings.FolderOptions);
            this.CheckBoxCaptionFolder.Checked = MySettings.FolderCaption;
            this.CombotoolbarPosition.SelectedIndex = MySettings.TollbarDockPosition;
            this.comboLanguage.SelectedIndex = MySettings.LanguageSelected ;
            this.checkFolderWatcher.Checked = MySettings.FolderActiveControl;
            this.ToolbarWidthHeight.Value = Math.Min(Math.Max(MySettings.ToolbarHeightWidth, this.ToolbarWidthHeight.Minimum), this.ToolbarWidthHeight.Maximum);
            this.CheckboxLargeIcons.Checked = MySettings.LargeIcons;
            this.CheckboxAutoHide.Checked = MySettings.AutoHide;
            this.NumericAutoHideInterval.Value = Math.Min(Math.Max(MySettings.AutoHideInterval, this.NumericAutoHideInterval.Minimum), this.NumericAutoHideInterval.Maximum);
            this.CheckboxShowText.Checked = MySettings.ShowText;
            this.CheckboxAero.Checked = MySettings.GlassStyleEffect && VistaApi.IsGlassEnabled();
            this.CheckShowHidden.Checked = MySettings.ShowHidden;
            this._SelectedFont = SettingsHelper.ParseFont(MySettings.TextFont);
            this._SelectedColor = SettingsHelper.ParseColor(MySettings.TextColor);
            this.ComboOverflow.SelectedIndex = Math.Min(Math.Max(0, MySettings.OverflowMode), this.ComboOverflow.Items.Count - 1);
            this.NumericOverflowColumns.Value = Math.Min(Math.Max(1, MySettings.OverflowColumns), 10);
        }

        private SettingsDraft CaptureDraft()
        {
            SettingsDraft d = new SettingsDraft();
            foreach (object AttItem in this.ListFolders.Items)
            {
                d.Folders.Add(AttItem.ToString());
            }
            d.FolderOptions = this._FolderOptions;
            d.FolderCaption = this.CheckBoxCaptionFolder.Checked;
            d.DockPosition = this.CombotoolbarPosition.SelectedIndex;
            d.Language = this.comboLanguage.SelectedIndex;
            d.FolderWatcher = this.checkFolderWatcher.Checked;
            d.ToolbarSize = this.ToolbarWidthHeight.Value;
            d.LargeIcons = this.CheckboxLargeIcons.Checked;
            d.AutoHide = this.CheckboxAutoHide.Checked;
            d.AutoHideInterval = this.NumericAutoHideInterval.Value;
            d.ShowText = this.CheckboxShowText.Checked;
            d.Aero = this.CheckboxAero.Checked;
            d.ShowHidden = this.CheckShowHidden.Checked;
            d.TextFont = this._SelectedFont;
            d.TextColor = this._SelectedColor;
            d.OverflowMode = this.ComboOverflow.SelectedIndex;
            d.OverflowColumns = this.NumericOverflowColumns.Value;
            return d;
        }

        private void LoadFromDraft(SettingsDraft d)
        {
            foreach (string AttFolder in d.Folders)
            {
                this.ListFolders.Items.Add(AttFolder);
            }
            this._FolderOptions = d.FolderOptions;
            this.CheckBoxCaptionFolder.Checked = d.FolderCaption;
            this.CombotoolbarPosition.SelectedIndex = d.DockPosition;
            this.comboLanguage.SelectedIndex = d.Language;
            this.checkFolderWatcher.Checked = d.FolderWatcher;
            this.ToolbarWidthHeight.Value = d.ToolbarSize;
            this.CheckboxLargeIcons.Checked = d.LargeIcons;
            this.CheckboxAutoHide.Checked = d.AutoHide;
            this.NumericAutoHideInterval.Value = d.AutoHideInterval;
            this.CheckboxShowText.Checked = d.ShowText;
            this.CheckboxAero.Checked = d.Aero;
            this.CheckShowHidden.Checked = d.ShowHidden;
            this._SelectedFont = d.TextFont;
            this._SelectedColor = d.TextColor;
            this.ComboOverflow.SelectedIndex = Math.Max(0, d.OverflowMode);
            this.NumericOverflowColumns.Value = d.OverflowColumns;
        }

        /// <summary>
        /// Modifica percorso e opzioni specifiche della cartella selezionata (ticket #16).
        /// Senza selezione aggiunge una nuova cartella.
        /// </summary>
        private void EditFolder()
        {
            string AttPath = "";
            int Index = ListFolders.SelectedIndex;
            if (Index >= 0)
            {
                AttPath = ListFolders.Items[Index].ToString();
            }
            FolderOptions Opt;
            if (!this._FolderOptions.TryGetValue(AttPath, out Opt))
            {
                Opt = new FolderOptions();
                Opt.Path = AttPath;
            }
            using (FolderOptionsDialog Dlg = new FolderOptionsDialog(Opt))
            {
                if (Dlg.ShowDialog(this) == DialogResult.OK)
                {
                    if (!String.IsNullOrEmpty(AttPath))
                    {
                        this._FolderOptions.Remove(AttPath);
                    }
                    this._FolderOptions[Dlg.Result.Path] = Dlg.Result;
                    if (Index >= 0)
                    {
                        ListFolders.Items[Index] = Dlg.Result.Path;
                    }
                    else
                    {
                        ListFolders.Items.Add(Dlg.Result.Path);
                    }
                }
            }
        }

        private void ListFolders_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.EditFolder();
            }
        }

        private void ButtonFolderOptions_Click(object sender, EventArgs e)
        {
            this.EditFolder();
        }

        private void comboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnChangingLang || this.comboLanguage.SelectedIndex < 0)
            {
                return;
            }
            string Culture;
            switch (this.comboLanguage.SelectedIndex)
            {
                case (int)ApplLanguage.English:
                    Culture = "en-US";
                    break;
                case (int)ApplLanguage.Francais:
                    Culture = "fr-FR";
                    break;
                case (int)ApplLanguage.Espanol:
                    Culture = "es-ES";
                    break;
                case (int)ApplLanguage.Deutsch:
                    Culture = "de-DE";
                    break;
                default:
                    Culture = "it-IT";
                    break;
            }
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Culture);
            //la finestra viene ricreata nella nuova lingua (vedi SettingsDraft)
            this.Draft = this.CaptureDraft();
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void CombotoolbarPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabelWidth.Visible = CombotoolbarPosition.SelectedIndex == 0 || CombotoolbarPosition.SelectedIndex == 2;
            LabelHeight.Visible = !LabelWidth.Visible;
        }

        private void CheckboxLargeIcons_CheckedChanged(object sender, EventArgs e)
        {
            if (OnChangingLang)
            {
                return;
            }
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
