using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SevenDeskBar
{
    /// <summary>
    /// Impostazioni specifiche di una cartella inclusa (ticket #16 e #22).
    /// I valori null significano "usa l'impostazione generale".
    /// Salvate nell'impostazione FolderOptions come
    ///   percorso*titolo*testo*iconegrandi*allineafine|percorso*...
    /// ("" = generale, "1" = sì, "0" = no). I caratteri | e * non possono
    /// comparire nei percorsi di Windows.
    /// </summary>
    public class FolderOptions
    {
        public string Path = "";
        public bool? Caption;
        public bool? ShowText;
        public bool? LargeIcons;
        public bool AlignEnd;

        public bool IsDefault
        {
            get { return !Caption.HasValue && !ShowText.HasValue && !LargeIcons.HasValue && !AlignEnd; }
        }

        public FolderOptions Clone()
        {
            return (FolderOptions)this.MemberwiseClone();
        }

        private static string ToField(bool? Value)
        {
            return Value.HasValue ? (Value.Value ? "1" : "0") : "";
        }

        private static bool? FromField(string Value)
        {
            if (Value == "1") return true;
            if (Value == "0") return false;
            return null;
        }

        public static Dictionary<string, FolderOptions> Parse(string Value)
        {
            Dictionary<string, FolderOptions> Result = new Dictionary<string, FolderOptions>(StringComparer.OrdinalIgnoreCase);
            if (String.IsNullOrEmpty(Value))
            {
                return Result;
            }
            foreach (string Entry in Value.Split('|'))
            {
                string[] Fields = Entry.Split('*');
                if (Fields.Length < 5 || String.IsNullOrEmpty(Fields[0]))
                {
                    continue;
                }
                FolderOptions Opt = new FolderOptions();
                Opt.Path = Fields[0];
                Opt.Caption = FromField(Fields[1]);
                Opt.ShowText = FromField(Fields[2]);
                Opt.LargeIcons = FromField(Fields[3]);
                Opt.AlignEnd = Fields[4] == "1";
                Result[Opt.Path] = Opt;
            }
            return Result;
        }

        public static string Serialize(IEnumerable<FolderOptions> Options)
        {
            StringBuilder sb = new StringBuilder();
            foreach (FolderOptions Opt in Options)
            {
                if (Opt == null || Opt.IsDefault || String.IsNullOrEmpty(Opt.Path))
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    sb.Append('|');
                }
                sb.Append(Opt.Path).Append('*')
                  .Append(ToField(Opt.Caption)).Append('*')
                  .Append(ToField(Opt.ShowText)).Append('*')
                  .Append(ToField(Opt.LargeIcons)).Append('*')
                  .Append(Opt.AlignEnd ? "1" : "0");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Finestra per modificare percorso e opzioni di una cartella inclusa.
    /// Costruita come il codice del Designer (AutoScaleMode.Font) così si adatta al DPI.
    /// </summary>
    public class FolderOptionsDialog : Form
    {
        private Label LabelPath;
        private TextBox TextPath;
        private Button ButtonBrowse;
        private CheckBox CheckCaption;
        private CheckBox CheckText;
        private CheckBox CheckLarge;
        private CheckBox CheckEnd;
        private Label LabelHint;
        private Button ButtonOk;
        private Button ButtonCancel;

        public FolderOptions Result;

        public FolderOptionsDialog(FolderOptions Options)
        {
            this.Result = Options != null ? Options.Clone() : new FolderOptions();
            this.InitializeComponent();
            this.Text = Msg("FolderOptionsTitle");
            this.LabelPath.Text = Msg("FolderOptionsPath");
            this.ButtonBrowse.Text = Msg("FolderOptionsBrowse");
            this.CheckCaption.Text = Msg("FolderOptionsCaption");
            this.CheckText.Text = Msg("FolderOptionsText");
            this.CheckLarge.Text = Msg("FolderOptionsLarge");
            this.CheckEnd.Text = Msg("FolderOptionsEnd");
            this.LabelHint.Text = Msg("FolderOptionsHint");
            this.ButtonOk.Text = Msg("FolderOptionsSave");
            this.ButtonCancel.Text = Msg("FolderOptionsCancel");

            this.TextPath.Text = this.Result.Path;
            this.CheckCaption.CheckState = ToState(this.Result.Caption);
            this.CheckText.CheckState = ToState(this.Result.ShowText);
            this.CheckLarge.CheckState = ToState(this.Result.LargeIcons);
            this.CheckEnd.Checked = this.Result.AlignEnd;
        }

        private static string Msg(string Key)
        {
            return ResourceMessages.ResourceManager.GetString(Key) ?? Key;
        }

        private static CheckState ToState(bool? Value)
        {
            if (!Value.HasValue) return CheckState.Indeterminate;
            return Value.Value ? CheckState.Checked : CheckState.Unchecked;
        }

        private static bool? FromState(CheckState State)
        {
            if (State == CheckState.Indeterminate) return null;
            return State == CheckState.Checked;
        }

        private void InitializeComponent()
        {
            this.LabelPath = new Label();
            this.TextPath = new TextBox();
            this.ButtonBrowse = new Button();
            this.CheckCaption = new CheckBox();
            this.CheckText = new CheckBox();
            this.CheckLarge = new CheckBox();
            this.CheckEnd = new CheckBox();
            this.LabelHint = new Label();
            this.ButtonOk = new Button();
            this.ButtonCancel = new Button();
            this.Font = Program.DialogFont();
            this.SuspendLayout();

            this.LabelPath.AutoSize = true;
            this.LabelPath.Location = new Point(12, 12);

            this.TextPath.Location = new Point(12, 30);
            this.TextPath.Size = new Size(315, 20);
            this.TextPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.TextPath.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.TextPath.AutoCompleteSource = AutoCompleteSource.FileSystemDirectories;

            this.ButtonBrowse.Location = new Point(333, 28);
            this.ButtonBrowse.Size = new Size(75, 23);
            this.ButtonBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.ButtonBrowse.UseVisualStyleBackColor = true;
            this.ButtonBrowse.Click += new EventHandler(this.ButtonBrowse_Click);

            this.CheckCaption.AutoSize = true;
            this.CheckCaption.ThreeState = true;
            this.CheckCaption.Location = new Point(12, 64);

            this.CheckText.AutoSize = true;
            this.CheckText.ThreeState = true;
            this.CheckText.Location = new Point(12, 88);

            this.CheckLarge.AutoSize = true;
            this.CheckLarge.ThreeState = true;
            this.CheckLarge.Location = new Point(12, 112);

            this.CheckEnd.AutoSize = true;
            this.CheckEnd.Location = new Point(12, 136);

            this.LabelHint.Location = new Point(12, 162);
            this.LabelHint.Size = new Size(396, 28);
            this.LabelHint.ForeColor = SystemColors.GrayText;

            this.ButtonOk.Location = new Point(252, 196);
            this.ButtonOk.Size = new Size(75, 23);
            this.ButtonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new EventHandler(this.ButtonOk_Click);

            this.ButtonCancel.Location = new Point(333, 196);
            this.ButtonCancel.Size = new Size(75, 23);
            this.ButtonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.DialogResult = DialogResult.Cancel;

            this.AcceptButton = this.ButtonOk;
            this.CancelButton = this.ButtonCancel;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(420, 230);
            this.Controls.Add(this.LabelPath);
            this.Controls.Add(this.TextPath);
            this.Controls.Add(this.ButtonBrowse);
            this.Controls.Add(this.CheckCaption);
            this.Controls.Add(this.CheckText);
            this.Controls.Add(this.CheckLarge);
            this.Controls.Add(this.CheckEnd);
            this.Controls.Add(this.LabelHint);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonCancel);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Name = "FolderOptionsDialog";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog Dlg = new FolderBrowserDialog())
            {
                if (Directory.Exists(this.TextPath.Text))
                {
                    Dlg.SelectedPath = this.TextPath.Text;
                }
                if (Dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this.TextPath.Text = Dlg.SelectedPath;
                }
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(this.TextPath.Text))
            {
                MessageBox.Show(ResourceMessages.ErrorInvalidPath, ResourceMessages.GenericErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            this.Result.Path = this.TextPath.Text;
            this.Result.Caption = FromState(this.CheckCaption.CheckState);
            this.Result.ShowText = FromState(this.CheckText.CheckState);
            this.Result.LargeIcons = FromState(this.CheckLarge.CheckState);
            this.Result.AlignEnd = this.CheckEnd.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
