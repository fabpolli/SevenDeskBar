namespace SevenDeskBar
{
    partial class MainForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Liberare le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.ContextMenuFolders = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copiaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spostaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.annullaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuForm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.impostazioniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OrderModetoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aggiornaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.mostraTestoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mostratitoloToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nascondiautomaticamenteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.informazionisuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.esciToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.timerAutoHide = new System.Windows.Forms.Timer(this.components);
            this.MainToolbar = new System.Windows.Forms.ToolStrip();
            this.ContextMenuFolders.SuspendLayout();
            this.ContextMenuForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContextMenuFolders
            // 
            this.ContextMenuFolders.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copiaToolStripMenuItem,
            this.spostaToolStripMenuItem,
            this.toolStripSeparator1,
            this.annullaToolStripMenuItem});
            this.ContextMenuFolders.Name = "ContextMenuFolders";
            resources.ApplyResources(this.ContextMenuFolders, "ContextMenuFolders");
            this.ContextMenuFolders.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuFolders_Opening);
            // 
            // copiaToolStripMenuItem
            // 
            this.copiaToolStripMenuItem.Name = "copiaToolStripMenuItem";
            resources.ApplyResources(this.copiaToolStripMenuItem, "copiaToolStripMenuItem");
            this.copiaToolStripMenuItem.Click += new System.EventHandler(this.copiaToolStripMenuItem_Click);
            // 
            // spostaToolStripMenuItem
            // 
            this.spostaToolStripMenuItem.Name = "spostaToolStripMenuItem";
            resources.ApplyResources(this.spostaToolStripMenuItem, "spostaToolStripMenuItem");
            this.spostaToolStripMenuItem.Click += new System.EventHandler(this.spostaToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // annullaToolStripMenuItem
            // 
            this.annullaToolStripMenuItem.Name = "annullaToolStripMenuItem";
            resources.ApplyResources(this.annullaToolStripMenuItem, "annullaToolStripMenuItem");
            // 
            // ContextMenuForm
            // 
            this.ContextMenuForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.impostazioniToolStripMenuItem,
            this.OrderModetoolStripMenuItem,
            this.aggiornaToolStripMenuItem,
            this.toolStripSeparator3,
            this.mostraTestoToolStripMenuItem,
            this.mostratitoloToolStripMenuItem,
            this.nascondiautomaticamenteToolStripMenuItem,
            this.toolStripSeparator4,
            this.informazionisuToolStripMenuItem,
            this.toolStripSeparator2,
            this.esciToolStripMenuItem});
            this.ContextMenuForm.Name = "ContextMenuForm";
            resources.ApplyResources(this.ContextMenuForm, "ContextMenuForm");
            this.ContextMenuForm.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.ContextMenuForm_Closed);
            this.ContextMenuForm.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuForm_Opening);
            // 
            // impostazioniToolStripMenuItem
            // 
            this.impostazioniToolStripMenuItem.Name = "impostazioniToolStripMenuItem";
            resources.ApplyResources(this.impostazioniToolStripMenuItem, "impostazioniToolStripMenuItem");
            this.impostazioniToolStripMenuItem.Click += new System.EventHandler(this.impostazioniToolStripMenuItem_Click);
            // 
            // OrderModetoolStripMenuItem
            // 
            this.OrderModetoolStripMenuItem.Name = "OrderModetoolStripMenuItem";
            resources.ApplyResources(this.OrderModetoolStripMenuItem, "OrderModetoolStripMenuItem");
            this.OrderModetoolStripMenuItem.Click += new System.EventHandler(this.OrderModetoolStripMenuItem_Click);
            // 
            // aggiornaToolStripMenuItem
            // 
            this.aggiornaToolStripMenuItem.Name = "aggiornaToolStripMenuItem";
            resources.ApplyResources(this.aggiornaToolStripMenuItem, "aggiornaToolStripMenuItem");
            this.aggiornaToolStripMenuItem.Click += new System.EventHandler(this.aggiornaToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // mostraTestoToolStripMenuItem
            // 
            this.mostraTestoToolStripMenuItem.Name = "mostraTestoToolStripMenuItem";
            resources.ApplyResources(this.mostraTestoToolStripMenuItem, "mostraTestoToolStripMenuItem");
            this.mostraTestoToolStripMenuItem.Click += new System.EventHandler(this.mostraTestoToolStripMenuItem_Click);
            // 
            // mostratitoloToolStripMenuItem
            // 
            this.mostratitoloToolStripMenuItem.Name = "mostratitoloToolStripMenuItem";
            resources.ApplyResources(this.mostratitoloToolStripMenuItem, "mostratitoloToolStripMenuItem");
            this.mostratitoloToolStripMenuItem.Click += new System.EventHandler(this.mostratitoloToolStripMenuItem_Click);
            // 
            // nascondiautomaticamenteToolStripMenuItem
            // 
            this.nascondiautomaticamenteToolStripMenuItem.Name = "nascondiautomaticamenteToolStripMenuItem";
            resources.ApplyResources(this.nascondiautomaticamenteToolStripMenuItem, "nascondiautomaticamenteToolStripMenuItem");
            this.nascondiautomaticamenteToolStripMenuItem.Click += new System.EventHandler(this.nascondiautomaticamenteToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // informazionisuToolStripMenuItem
            // 
            this.informazionisuToolStripMenuItem.Name = "informazionisuToolStripMenuItem";
            resources.ApplyResources(this.informazionisuToolStripMenuItem, "informazionisuToolStripMenuItem");
            this.informazionisuToolStripMenuItem.Click += new System.EventHandler(this.informazionisuToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // esciToolStripMenuItem
            // 
            this.esciToolStripMenuItem.Name = "esciToolStripMenuItem";
            resources.ApplyResources(this.esciToolStripMenuItem, "esciToolStripMenuItem");
            this.esciToolStripMenuItem.Click += new System.EventHandler(this.esciToolStripMenuItem_Click);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Interval = 1;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // timerAutoHide
            // 
            this.timerAutoHide.Interval = 250;
            this.timerAutoHide.Tick += new System.EventHandler(this.timerAutoHide_Tick);
            // 
            // MainToolbar
            // 
            this.MainToolbar.AllowDrop = true;
            resources.ApplyResources(this.MainToolbar, "MainToolbar");
            this.MainToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainToolbar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.MainToolbar.Name = "MainToolbar";
            this.MainToolbar.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.MainToolbar.MouseEnter += new System.EventHandler(this.MainToolbar_MouseEnter);
            this.MainToolbar.MouseLeave += new System.EventHandler(this.MainToolbar_MouseLeave);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.ContextMenuForm;
            this.ControlBox = false;
            this.Controls.Add(this.MainToolbar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MouseEnter += new System.EventHandler(this.MainForm_MouseEnter);
            this.DoubleClick += new System.EventHandler(this.MainForm_DoubleClick);
            this.MouseLeave += new System.EventHandler(this.MainForm_MouseLeave);
            this.ContextMenuFolders.ResumeLayout(false);
            this.ContextMenuForm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem copiaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem spostaToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem annullaToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ContextMenuForm;
        private System.Windows.Forms.ToolStripMenuItem impostazioniToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem informazionisuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem esciToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ContextMenuFolders;
        private System.Windows.Forms.ToolStripMenuItem aggiornaToolStripMenuItem;
        public System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Timer timerAutoHide;
        private System.Windows.Forms.ToolStrip MainToolbar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mostraTestoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mostratitoloToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem nascondiautomaticamenteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OrderModetoolStripMenuItem;
    }
}