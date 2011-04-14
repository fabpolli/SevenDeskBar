namespace SevenDeskBar
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            this.label1 = new System.Windows.Forms.Label();
            this.ListFolders = new System.Windows.Forms.ListBox();
            this.FolderBrowserAdd = new System.Windows.Forms.FolderBrowserDialog();
            this.ButtonAdd = new System.Windows.Forms.Button();
            this.ButtonRemove = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.ButtonSave = new System.Windows.Forms.Button();
            this.CheckBoxCaptionFolder = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.CombotoolbarPosition = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboLanguage = new System.Windows.Forms.ComboBox();
            this.checkFolderWatcher = new System.Windows.Forms.CheckBox();
            this.LabelWidth = new System.Windows.Forms.Label();
            this.ToolbarWidthHeight = new System.Windows.Forms.NumericUpDown();
            this.LabelHeight = new System.Windows.Forms.Label();
            this.CheckboxLargeIcons = new System.Windows.Forms.CheckBox();
            this.CheckboxAutoHide = new System.Windows.Forms.CheckBox();
            this.NumericAutoHideInterval = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.CheckboxShowText = new System.Windows.Forms.CheckBox();
            this.ButtonDeleteOrderData = new System.Windows.Forms.Button();
            this.CheckboxAero = new System.Windows.Forms.CheckBox();
            this.buttonDown = new System.Windows.Forms.Button();
            this.buttonUp = new System.Windows.Forms.Button();
            this.toolTipFolderMove = new System.Windows.Forms.ToolTip(this.components);
            this.CheckShowHidden = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ToolbarWidthHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAutoHideInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // ListFolders
            // 
            resources.ApplyResources(this.ListFolders, "ListFolders");
            this.ListFolders.FormattingEnabled = true;
            this.ListFolders.Name = "ListFolders";
            this.ListFolders.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.ListFolders.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListFolders_MouseDoubleClick);
            this.ListFolders.SelectedIndexChanged += new System.EventHandler(this.ListFolders_SelectedIndexChanged);
            // 
            // FolderBrowserAdd
            // 
            resources.ApplyResources(this.FolderBrowserAdd, "FolderBrowserAdd");
            this.FolderBrowserAdd.ShowNewFolderButton = false;
            // 
            // ButtonAdd
            // 
            resources.ApplyResources(this.ButtonAdd, "ButtonAdd");
            this.ButtonAdd.Name = "ButtonAdd";
            this.ButtonAdd.UseVisualStyleBackColor = true;
            this.ButtonAdd.Click += new System.EventHandler(this.ButtonAdd_Click);
            // 
            // ButtonRemove
            // 
            resources.ApplyResources(this.ButtonRemove, "ButtonRemove");
            this.ButtonRemove.Name = "ButtonRemove";
            this.ButtonRemove.UseVisualStyleBackColor = true;
            this.ButtonRemove.Click += new System.EventHandler(this.ButtonRemove_Click);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // ButtonSave
            // 
            resources.ApplyResources(this.ButtonSave, "ButtonSave");
            this.ButtonSave.Name = "ButtonSave";
            this.ButtonSave.UseVisualStyleBackColor = true;
            this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // CheckBoxCaptionFolder
            // 
            resources.ApplyResources(this.CheckBoxCaptionFolder, "CheckBoxCaptionFolder");
            this.CheckBoxCaptionFolder.Checked = true;
            this.CheckBoxCaptionFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CheckBoxCaptionFolder.Name = "CheckBoxCaptionFolder";
            this.CheckBoxCaptionFolder.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // CombotoolbarPosition
            // 
            resources.ApplyResources(this.CombotoolbarPosition, "CombotoolbarPosition");
            this.CombotoolbarPosition.FormattingEnabled = true;
            this.CombotoolbarPosition.Items.AddRange(new object[] {
            resources.GetString("CombotoolbarPosition.Items"),
            resources.GetString("CombotoolbarPosition.Items1"),
            resources.GetString("CombotoolbarPosition.Items2"),
            resources.GetString("CombotoolbarPosition.Items3")});
            this.CombotoolbarPosition.Name = "CombotoolbarPosition";
            this.CombotoolbarPosition.SelectedIndexChanged += new System.EventHandler(this.CombotoolbarPosition_SelectedIndexChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // comboLanguage
            // 
            resources.ApplyResources(this.comboLanguage, "comboLanguage");
            this.comboLanguage.FormattingEnabled = true;
            this.comboLanguage.Items.AddRange(new object[] {
            resources.GetString("comboLanguage.Items"),
            resources.GetString("comboLanguage.Items1"),
            resources.GetString("comboLanguage.Items2"),
            resources.GetString("comboLanguage.Items3"),
            resources.GetString("comboLanguage.Items4")});
            this.comboLanguage.Name = "comboLanguage";
            this.comboLanguage.SelectedIndexChanged += new System.EventHandler(this.comboLanguage_SelectedIndexChanged);
            // 
            // checkFolderWatcher
            // 
            resources.ApplyResources(this.checkFolderWatcher, "checkFolderWatcher");
            this.checkFolderWatcher.Name = "checkFolderWatcher";
            this.checkFolderWatcher.UseVisualStyleBackColor = true;
            // 
            // LabelWidth
            // 
            resources.ApplyResources(this.LabelWidth, "LabelWidth");
            this.LabelWidth.Name = "LabelWidth";
            // 
            // ToolbarWidthHeight
            // 
            resources.ApplyResources(this.ToolbarWidthHeight, "ToolbarWidthHeight");
            this.ToolbarWidthHeight.Maximum = new decimal(new int[] {
            400,
            0,
            0,
            0});
            this.ToolbarWidthHeight.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ToolbarWidthHeight.Name = "ToolbarWidthHeight";
            this.ToolbarWidthHeight.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // LabelHeight
            // 
            resources.ApplyResources(this.LabelHeight, "LabelHeight");
            this.LabelHeight.Name = "LabelHeight";
            // 
            // CheckboxLargeIcons
            // 
            resources.ApplyResources(this.CheckboxLargeIcons, "CheckboxLargeIcons");
            this.CheckboxLargeIcons.Name = "CheckboxLargeIcons";
            this.CheckboxLargeIcons.UseVisualStyleBackColor = true;
            this.CheckboxLargeIcons.CheckedChanged += new System.EventHandler(this.CheckboxLargeIcons_CheckedChanged);
            // 
            // CheckboxAutoHide
            // 
            resources.ApplyResources(this.CheckboxAutoHide, "CheckboxAutoHide");
            this.CheckboxAutoHide.Name = "CheckboxAutoHide";
            this.CheckboxAutoHide.UseVisualStyleBackColor = true;
            // 
            // NumericAutoHideInterval
            // 
            resources.ApplyResources(this.NumericAutoHideInterval, "NumericAutoHideInterval");
            this.NumericAutoHideInterval.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.NumericAutoHideInterval.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.NumericAutoHideInterval.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.NumericAutoHideInterval.Name = "NumericAutoHideInterval";
            this.NumericAutoHideInterval.Value = new decimal(new int[] {
            250,
            0,
            0,
            0});
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // CheckboxShowText
            // 
            resources.ApplyResources(this.CheckboxShowText, "CheckboxShowText");
            this.CheckboxShowText.Name = "CheckboxShowText";
            this.CheckboxShowText.UseVisualStyleBackColor = true;
            // 
            // ButtonDeleteOrderData
            // 
            resources.ApplyResources(this.ButtonDeleteOrderData, "ButtonDeleteOrderData");
            this.ButtonDeleteOrderData.Name = "ButtonDeleteOrderData";
            this.ButtonDeleteOrderData.UseVisualStyleBackColor = true;
            this.ButtonDeleteOrderData.Click += new System.EventHandler(this.ButtonDeleteOrderData_Click);
            // 
            // CheckboxAero
            // 
            resources.ApplyResources(this.CheckboxAero, "CheckboxAero");
            this.CheckboxAero.Name = "CheckboxAero";
            this.CheckboxAero.UseVisualStyleBackColor = true;
            // 
            // buttonDown
            // 
            resources.ApplyResources(this.buttonDown, "buttonDown");
            this.buttonDown.Name = "buttonDown";
            this.toolTipFolderMove.SetToolTip(this.buttonDown, resources.GetString("buttonDown.ToolTip"));
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
            // 
            // buttonUp
            // 
            resources.ApplyResources(this.buttonUp, "buttonUp");
            this.buttonUp.Name = "buttonUp";
            this.toolTipFolderMove.SetToolTip(this.buttonUp, resources.GetString("buttonUp.ToolTip"));
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
            // 
            // toolTipFolderMove
            // 
            this.toolTipFolderMove.IsBalloon = true;
            this.toolTipFolderMove.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.toolTipFolderMove.ToolTipTitle = "SevenDeskBar";
            // 
            // CheckShowHidden
            // 
            resources.ApplyResources(this.CheckShowHidden, "CheckShowHidden");
            this.CheckShowHidden.Name = "CheckShowHidden";
            this.CheckShowHidden.UseVisualStyleBackColor = true;
            // 
            // FormSettings
            // 
            this.AcceptButton = this.ButtonSave;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.Controls.Add(this.CheckShowHidden);
            this.Controls.Add(this.buttonUp);
            this.Controls.Add(this.buttonDown);
            this.Controls.Add(this.CheckboxAero);
            this.Controls.Add(this.ButtonDeleteOrderData);
            this.Controls.Add(this.CheckboxShowText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.NumericAutoHideInterval);
            this.Controls.Add(this.CheckboxAutoHide);
            this.Controls.Add(this.CheckboxLargeIcons);
            this.Controls.Add(this.LabelHeight);
            this.Controls.Add(this.ToolbarWidthHeight);
            this.Controls.Add(this.LabelWidth);
            this.Controls.Add(this.checkFolderWatcher);
            this.Controls.Add(this.comboLanguage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.CombotoolbarPosition);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CheckBoxCaptionFolder);
            this.Controls.Add(this.ButtonSave);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.ButtonRemove);
            this.Controls.Add(this.ButtonAdd);
            this.Controls.Add(this.ListFolders);
            this.Controls.Add(this.label1);
            this.Name = "FormSettings";
            this.Load += new System.EventHandler(this.FormSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ToolbarWidthHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAutoHideInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox ListFolders;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserAdd;
        private System.Windows.Forms.Button ButtonAdd;
        private System.Windows.Forms.Button ButtonRemove;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button ButtonSave;
        private System.Windows.Forms.CheckBox CheckBoxCaptionFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox CombotoolbarPosition;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.CheckBox checkFolderWatcher;
        private System.Windows.Forms.Label LabelWidth;
        private System.Windows.Forms.NumericUpDown ToolbarWidthHeight;
        private System.Windows.Forms.Label LabelHeight;
        private System.Windows.Forms.CheckBox CheckboxLargeIcons;
        private System.Windows.Forms.CheckBox CheckboxAutoHide;
        private System.Windows.Forms.NumericUpDown NumericAutoHideInterval;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox CheckboxShowText;
        private System.Windows.Forms.Button ButtonDeleteOrderData;
        private System.Windows.Forms.CheckBox CheckboxAero;
        private System.Windows.Forms.Button buttonDown;
        private System.Windows.Forms.Button buttonUp;
        private System.Windows.Forms.ToolTip toolTipFolderMove;
        private System.Windows.Forms.CheckBox CheckShowHidden;
    }
}