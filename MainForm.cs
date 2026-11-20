using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.IO.IsolatedStorage;


namespace SevenDeskBar
{

    public partial class MainForm : DesktopToolbar 
    {
        private string[] _ParamList;
        private string _DestinationPath;
        private bool _AutoHide;
        private int _AutoHideInterval;
        private bool ToolbarIsHide = false;
        private bool _RightClickFired = false;
        const string UserConfigFile = "SevenDeskBarIconOrder.dat";
        private int _OldMilliseconds = 0;
        private Color _OldBackColor;
        private Color _OldToolbarBackColor;
        private Font _DefaultToolbarFont;
        private Color _DefaultToolbarForeColor;
        private Color _CustomTextColor = Color.Empty;
        private readonly List<FileSystemWatcher> _Watchers = new List<FileSystemWatcher>();

        // Gestione cambi di risoluzione / zoom / monitor
        private const int WM_DISPLAYCHANGE = 0x007E;
        private const int WM_SETTINGCHANGE = 0x001A;
        private const int WM_DPICHANGED = 0x02E0;
        private System.Windows.Forms.Timer _DisplayChangeTimer;
        private string _DisplaySignature;

        delegate void DisposeToolbarObjectsCallback();

        public MainForm()
        {
            LanguageManager.ChangeLanguage(this);
            InitializeComponent();
            // WS_EX_TOOLWINDOW (esclusione da Alt+Tab) ora è impostato in DesktopToolbar.CreateParams
            this._DefaultToolbarFont = this.MainToolbar.Font;
            this._DefaultToolbarForeColor = this.MainToolbar.ForeColor;

            this._DisplayChangeTimer = new System.Windows.Forms.Timer();
            this._DisplayChangeTimer.Interval = 800;
            this._DisplayChangeTimer.Tick += new EventHandler(this.DisplayChangeTimer_Tick);
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(this.SystemDisplayEvent);
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(this.SystemDisplayEvent);

            this.InitOverflowHandling();
            this.InitDragDropHandling();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= new EventHandler(this.SystemDisplayEvent);
            Microsoft.Win32.SystemEvents.UserPreferenceChanged -= new Microsoft.Win32.UserPreferenceChangedEventHandler(this.SystemDisplayEvent);
            this._DisplayChangeTimer.Stop();
            this.DisposeWatchers();
            base.OnFormClosed(e);
        }

        #region Adeguamento a risoluzione / zoom / monitor

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_DPICHANGED)
            {
                //Cambio di zoom del monitor: la barra viene ridisegnata da LoadObjects con le
                //nuove misure. Il messaggio non viene passato a Windows Forms, che altrimenti
                //ridimensionerebbe e sposterebbe la finestra per conto suo.
                this.ScheduleDisplayCheck();
                m.Result = IntPtr.Zero;
                return;
            }
            if (m.Msg == WM_DISPLAYCHANGE || m.Msg == WM_SETTINGCHANGE)
            {
                this.ScheduleDisplayCheck();
            }
            base.WndProc(ref m);
        }

        private void SystemDisplayEvent(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(this.ScheduleDisplayCheck));
            }
            else
            {
                this.ScheduleDisplayCheck();
            }
        }

        private void ScheduleDisplayCheck()
        {
            if (this._DisplayChangeTimer == null)
            {
                return;
            }
            // più eventi ravvicinati (cambio risoluzione + DPI + area di lavoro) => un solo ridisegno
            this._DisplayChangeTimer.Stop();
            this._DisplayChangeTimer.Start();
        }

        /// <summary>
        /// "Firma" della configurazione video: se non cambia non serve ridisegnare.
        /// Evita anche cicli infiniti, perché la toolbar stessa modifica l'area di lavoro
        /// dello schermo (e quindi genera WM_SETTINGCHANGE) quando si riposiziona.
        /// </summary>
        private string ComputeDisplaySignature()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Screen s in Screen.AllScreens)
            {
                sb.Append(s.DeviceName).Append(':').Append(s.Bounds.ToString()).Append(s.Primary ? "P" : "").Append(';');
            }
            sb.Append(SystemInformation.PrimaryMonitorSize.ToString());
            sb.Append(";dpi=").Append(DockMonitorDpi());
            return sb.ToString();
        }

        private void DisplayChangeTimer_Tick(object sender, EventArgs e)
        {
            this._DisplayChangeTimer.Stop();
            string NewSignature = this.ComputeDisplaySignature();
            if (NewSignature == this._DisplaySignature)
            {
                return;
            }
            this._DisplaySignature = NewSignature;
            this.CloseOverflowPopup();
            this.RefreshToolbar(true);
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            this._DisplaySignature = this.ComputeDisplaySignature();

            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            LanguageManager.ChangeLanguage(this);
            string FoldersToAnalize = MySettings.IncludedFolders;
            this._AutoHide = MySettings.AutoHide;
            this._AutoHideInterval = MySettings.AutoHideInterval;
            if (String.IsNullOrEmpty(FoldersToAnalize))
            {
                MySettings.Upgrade();
                MySettings.Reload();
                if (String.IsNullOrEmpty(MySettings.IncludedFolders))
                {
                    //impostazioni delle versioni 1.0.x (.NET 2.0), salvate in un'altra cartella
                    LegacySettings.ImportFromPreviousVersion(MySettings);
                    MySettings.Reload();
                }
                this._AutoHide = MySettings.AutoHide;
                this._AutoHideInterval = MySettings.AutoHideInterval;
                FoldersToAnalize = MySettings.IncludedFolders;
                if (String.IsNullOrEmpty(FoldersToAnalize))
                {
                    this.impostazioniToolStripMenuItem_Click(sender, e);
                    MySettings.Reload();
                    FoldersToAnalize = MySettings.IncludedFolders;
                    this._AutoHide = MySettings.AutoHide;
                    this._AutoHideInterval = MySettings.AutoHideInterval;
                    if (String.IsNullOrEmpty(FoldersToAnalize))
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    this.LoadObjects(true);
                    if (this._AutoHide)
                    {
                        this.timerAutoHide.Enabled = true;
                    }
                }
            }
            else
            {
                this.LoadObjects(true);
                if (this._AutoHide)
                {
                    this.timerAutoHide.Enabled = true;
                }
            }

        }

        private void CreateFolderWatcher(string FolderToWatch)
        {
            FileSystemWatcher watcher = new FileSystemWatcher();

            //Subscribe to the Created event.
            watcher.Created += new FileSystemEventHandler(watcher_FolderChange);
            watcher.Deleted += new FileSystemEventHandler(watcher_FolderChange);
            watcher.Changed += new FileSystemEventHandler(watcher_FolderChange);
            watcher.Renamed += new RenamedEventHandler(watcher_FolderChange);
            
            watcher.Path = FolderToWatch ;

            //Enable the FileSystemWatcher events.
            watcher.EnableRaisingEvents = true;
            lock (this._Watchers)
            {
                this._Watchers.Add(watcher);
            }
        }

        /// <summary>
        /// Rilascia i FileSystemWatcher creati al caricamento precedente
        /// (prima ad ogni aggiornamento della toolbar se ne aggiungevano di nuovi)
        /// </summary>
        private void DisposeWatchers()
        {
            lock (this._Watchers)
            {
                foreach (FileSystemWatcher AttWatch in this._Watchers)
                {
                    try
                    {
                        AttWatch.EnableRaisingEvents = false;
                        AttWatch.Dispose();
                    }
                    catch
                    {
                    }
                }
                this._Watchers.Clear();
            }
        }

        private void watcher_FolderChange(object sender, FileSystemEventArgs e)
        {
            if (sender is FileSystemWatcher)
            {
                FileSystemWatcher AttWatch = (FileSystemWatcher)sender;
                AttWatch.EnableRaisingEvents = false;
                lock (this._Watchers)
                {
                    this._Watchers.Remove(AttWatch);
                }
                AttWatch.Dispose();
            }
            if (this.InvokeRequired)
            {
                DisposeToolbarObjectsCallback d = new DisposeToolbarObjectsCallback(SetTimerOn);
                this.Invoke(d);
            }
            else
            {
                this.DisposeToolbarObjects();
            }
        }

        private void DisposeToolbarObjects()
        {
            while (this.MainToolbar.Items.Count > 0)
            {
                ToolStripItem AttItem = this.MainToolbar.Items[0];
                Image AttImage = AttItem.Image;
                AttItem.Dispose();
                if (AttImage != null)
                {
                    AttImage.Dispose();
                }
            }
            this._ItemFolder.Clear();
        }

        private void RefreshToolbar()
        {
            this.RefreshToolbar(false);
        }


        private void RefreshToolbar(bool ReEdge)
        {
            this.CloseOverflowPopup();
            this._ScrollOffset = 0;
            if (this.InvokeRequired)
            {
                DisposeToolbarObjectsCallback d = new DisposeToolbarObjectsCallback(DisposeToolbarObjects);
                this.Invoke(d);
            }
            else
            {
                this.DisposeToolbarObjects();
            }
            
            LanguageManager.ChangeLanguage(this);
            this.LoadObjects();
            if (ReEdge)
            {
                AppBarEdges OldEdge=this.Edge ;
                if (OldEdge != AppBarEdges.Float)
                {
                    this.Edge = AppBarEdges.Float;
                    this.Edge = OldEdge;
                }

            }
        }

        private void LoadObjects()
        {
            this.LoadObjects(false);
        }


        /// <summary>Opzioni effettive (generali + specifiche) usate per caricare una cartella</summary>
        private class FolderLoadOptions
        {
            public bool SetCaption;
            public bool ShowText;
            public bool LargeIcons;
            public bool AlignEnd;
            public bool FolderWatcher;
            public bool TopOrBottom;
            public bool EnableGlass;
            public bool ShowHidden;
        }

        // Fattore di zoom del monitor (1 = 100%, 1.5 = 150% ...) e font scalato corrente
        private float _Scale = 1f;
        private Font _ScaledToolbarFont;
        // cartella di appartenenza di ogni elemento della toolbar (per il drag & drop)
        private readonly Dictionary<ToolStripItem, string> _ItemFolder = new Dictionary<ToolStripItem, string>();

        private int Scaled(int Value)
        {
            return (int)Math.Round(Value * this._Scale);
        }

        private void LoadObjects(bool OnLoadingApp)
        {
            this.SuspendLayout();
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            string FoldersToAnalize = MySettings.IncludedFolders;
            string[] FolderArray = FoldersToAnalize.Split('|');
            Dictionary<string, FolderOptions> AllFolderOptions = FolderOptions.Parse(MySettings.FolderOptions);
            bool NoError = true;
            string ErrorMessages = "";
            this._AutoHide = MySettings.AutoHide;
            this._AutoHideInterval = MySettings.AutoHideInterval;
            this.ToolbarIsHide = !this._AutoHide;
            bool EnableGlass = MySettings.GlassStyleEffect;
            this._OverflowMode = MySettings.OverflowMode;
            this._OverflowColumns = Math.Max(1, MySettings.OverflowColumns);
            this.DisposeWatchers();
            this._ItemFolder.Clear();

            //Zoom (DPI) del monitor: tutte le misure sono in pixel fisici
            int MonitorDpi = DockMonitorDpi();
            this._Scale = MonitorDpi / 96f;
            int StandardHeightWidth = this.Scaled(MySettings.ToolbarHeightWidth);

            //Carattere e colore dei testi scelti dall'utente. Windows converte i punti in
            //pixel con il DPI "di sistema": se il monitor ha uno zoom diverso il font va scalato.
            Font BaseFont = SettingsHelper.ParseFont(MySettings.TextFont) ?? this._DefaultToolbarFont;
            Font OldScaledFont = this._ScaledToolbarFont;
            float FontFactor = MonitorDpi / (float)SystemDpi();
            this._ScaledToolbarFont = new Font(BaseFont.FontFamily, BaseFont.SizeInPoints * FontFactor, BaseFont.Style, GraphicsUnit.Point);
            this.MainToolbar.Font = this._ScaledToolbarFont;
            if (OldScaledFont != null)
            {
                OldScaledFont.Dispose();
            }
            this._CustomTextColor = SettingsHelper.ParseColor(MySettings.TextColor);
            this.MainToolbar.ForeColor = this._CustomTextColor.IsEmpty ? this._DefaultToolbarForeColor : this._CustomTextColor;

            this.MainToolbar.ImageScalingSize = new Size(this.Scaled(MySettings.LargeIcons ? 32 : 16), this.Scaled(MySettings.LargeIcons ? 32 : 16));
            Array AppBarEdgesValues = Enum.GetValues(typeof(AppBarEdges));
            AppBarEdges AttAppBarEdge = (AppBarEdges)AppBarEdgesValues.GetValue(Convert.ToInt32(MySettings.TollbarDockPosition));
            bool TopOrBottom = AttAppBarEdge == AppBarEdges.Top || AttAppBarEdge == AppBarEdges.Bottom;

            //il renderer decide elemento per elemento (testo sì/no, icone grandi/piccole)
            this.MainToolbar.Renderer = new MyRenderer();
            //la toolbar occupa sempre tutta l'area della finestra
            this.MainToolbar.MinimumSize = Size.Empty;
            this.MainToolbar.MaximumSize = Size.Empty;
            this.MainToolbar.Dock = DockStyle.Fill;
            this.MainToolbar.LayoutStyle = TopOrBottom ? ToolStripLayoutStyle.HorizontalStackWithOverflow : ToolStripLayoutStyle.VerticalStackWithOverflow;

            //Le cartelle "allineate alla fine" vanno in fondo alla collezione e in ordine
            //inverso: la ToolStrip dispone gli elementi allineati a destra/in basso
            //partendo dal bordo.
            List<ToolStripItem> StartItems = new List<ToolStripItem>();
            List<ToolStripItem> EndItems = new List<ToolStripItem>();
            foreach (string AttFolder in FolderArray)
            {
                if (!String.IsNullOrEmpty(AttFolder) && Directory.Exists(AttFolder))
                {
                    FolderOptions Opt;
                    AllFolderOptions.TryGetValue(AttFolder, out Opt);
                    FolderLoadOptions Load = new FolderLoadOptions();
                    Load.SetCaption = Opt != null && Opt.Caption.HasValue ? Opt.Caption.Value : MySettings.FolderCaption;
                    Load.ShowText = Opt != null && Opt.ShowText.HasValue ? Opt.ShowText.Value : MySettings.ShowText;
                    Load.LargeIcons = Opt != null && Opt.LargeIcons.HasValue ? Opt.LargeIcons.Value : MySettings.LargeIcons;
                    Load.AlignEnd = Opt != null && Opt.AlignEnd;
                    Load.FolderWatcher = MySettings.FolderActiveControl;
                    Load.TopOrBottom = TopOrBottom;
                    Load.EnableGlass = EnableGlass;
                    Load.ShowHidden = MySettings.ShowHidden;

                    List<ToolStripItem> Target = Load.AlignEnd ? EndItems : StartItems;
                    if (Target.Count > 0)
                    {
                        Target.Add(this.NewSeparator(Load.AlignEnd));
                    }
                    //Aggiungo controlli alla toolbar in base alle cartelle incluse
                    //(prima un errore impediva di caricare le cartelle successive)
                    bool FolderOk = AnalyzeFolder(AttFolder, Load, Target, ref ErrorMessages);
                    NoError = NoError && FolderOk;
                }
            }
            if (StartItems.Count > 0 && EndItems.Count > 0)
            {
                EndItems.Insert(0, this.NewSeparator(true));
            }
            EndItems.Reverse();
            this.MainToolbar.SuspendLayout();
            //Le sezioni allineate alla fine vanno PRIMA nella collezione: quando lo spazio
            //non basta la ToolStrip scarta gli ultimi elementi della collezione, che devono
            //essere quelli delle sezioni normali (finiscono nell'elenco » o si scorrono),
            //non la sezione fissata in fondo. La posizione a video dipende solo dall'allineamento.
            this.MainToolbar.Items.AddRange(EndItems.ToArray());
            this.MainToolbar.Items.AddRange(StartItems.ToArray());
            this.MainToolbar.ResumeLayout(false);

            //Pulsanti per elementi in eccesso (elenco a comparsa o scorrimento)
            this.CreateOverflowButtons(TopOrBottom);

            //Con l'auto-hide la barra non riserva spazio sullo schermo: altrimenti ogni
            //volta che compare/scompare tutte le finestre ingrandite vengono ridimensionate
            this.ReserveSpace = !this._AutoHide;
            if (!OnLoadingApp)
            {
                this.Edge = AttAppBarEdge;
            }
            if (TopOrBottom)
            {
                this.StandardHeight = StandardHeightWidth;
            }
            else
            {
                this.StandardWidth = StandardHeightWidth;
            }
            if (OnLoadingApp)
            {
                this.Edge = AttAppBarEdge;
            }
            if (VistaApi.IsGlassEnabled() && EnableGlass)
            {
                if (this._OldBackColor.IsEmpty)
                {
                    this._OldBackColor = this.BackColor;
                    this._OldToolbarBackColor = this.MainToolbar.BackColor;
                }
                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.Black;
                this.MainToolbar.BackColor = Color.Black;
                using (Graphics g = this.CreateGraphics())
                using (Region rgn = new Region(new Rectangle(0, 0, this.Width, this.Height)))
                {
                    VistaApi.DWM_BLURBEHIND bb = new VistaApi.DWM_BLURBEHIND();
                    bb.dwFlags = (VistaApi.DWM_BB_ENABLE | VistaApi.DWM_BB_BLURREGION);
                    bb.fEnable = true;
                    bb.hRgnBlur = rgn.GetHrgn(g);
                    // '#Enable blur-behind effect
                    VistaApi.DwmEnableBlurBehindWindow(this.Handle, ref bb);
                }
            }
            else
            {
                //bordo sottile da "tool window": Windows Forms non vi applica la larghezza
                //minima di sistema (oltre 100 pixel) e la barra può essere stretta
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
                if (!this._OldBackColor.IsEmpty)
                {
                    this.BackColor = this._OldBackColor;
                    this.MainToolbar.BackColor = this._OldToolbarBackColor;
                }
            }
            this.ResumeLayout();
            this.ApplyScroll();
            if (!NoError)
            {
                MessageBox.Show(ErrorMessages, ResourceMessages.ErrorToolbarCreation, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private ToolStripSeparator NewSeparator(bool AlignEnd)
        {
            ToolStripSeparator AttSep = new ToolStripSeparator();
            if (AlignEnd)
            {
                AttSep.Alignment = ToolStripItemAlignment.Right;
            }
            return AttSep;
        }

        /// <summary>
        /// Icona alla dimensione esatta richiesta. Prova nell'ordine: icone di sistema
        /// (anche 48/256 px per zoom elevati), icona del collegamento, icona dell'eseguibile.
        /// </summary>
        private Bitmap LoadItemBitmap(string ItemPath, bool IsFolder, int IconSize)
        {
            Bitmap Result = IconHelper.GetBitmap(ItemPath, IconSize);
            if (Result != null)
            {
                return Result;
            }
            if (!IsFolder && Path.GetExtension(ItemPath).ToUpper() == ".LNK")
            {
                ShellLink link = new ShellLink();
                try
                {
                    link.Open(ItemPath);
                    using (Icon FileIcon = IconSize > 16 ? link.LargeIcon : link.SmallIcon)
                    {
                        Result = IconHelper.FromIcon(FileIcon, IconSize);
                    }
                }
                catch
                {
                    Result = null;
                }
                finally
                {
                    link.Dispose();
                }
                if (Result != null)
                {
                    return Result;
                }
            }
            if (!IsFolder)
            {
                try
                {
                    using (Icon FileIcon = Icon.ExtractAssociatedIcon(ItemPath))
                    {
                        Result = IconHelper.FromIcon(FileIcon, IconSize);
                    }
                    if (Result != null)
                    {
                        return Result;
                    }
                }
                catch
                {
                }
            }
            using (Icon FileIcon = IconSize > 16 ? ShellIcon.GetLargeIcon(ItemPath) : ShellIcon.GetSmallIcon(ItemPath))
            {
                return IconHelper.FromIcon(FileIcon, IconSize);
            }
        }

        private bool AnalyzeFolder(string AttFolder, FolderLoadOptions Load, List<ToolStripItem> Target, ref string ErrorMessages)
        {
            bool NoError = true;
            ToolStripItemAlignment Alignment = Load.AlignEnd ? ToolStripItemAlignment.Right : ToolStripItemAlignment.Left;
            int IconSize = this.Scaled(Load.LargeIcons ? 32 : 16);

            if (Load.SetCaption)
            {
                ToolStripLabel AttLbl = new ToolStripLabel();
                AttLbl.Text = Path.GetFileName(AttFolder).ToString();
                if (String.IsNullOrEmpty(AttLbl.Text))
                {
                    //radice di un disco o di una condivisione di rete
                    AttLbl.Text = AttFolder;
                }
                AttLbl.TextAlign = ContentAlignment.MiddleLeft;
                AttLbl.ToolTipText = AttLbl.Text;
                AttLbl.Tag = AttFolder;
                AttLbl.Alignment = Alignment;
                AttLbl.Click += new EventHandler(this.ToolStripLabel_DoubleClick);
                if (!this._CustomTextColor.IsEmpty)
                {
                    AttLbl.ForeColor = this._CustomTextColor;
                }
                else if (Load.EnableGlass)
                {
                    AttLbl.ForeColor = Color.White;
                }
                Target.Add(AttLbl);
                this._ItemFolder[AttLbl] = AttFolder;
            }
            //Analizzo cartelle e file della directory corrente e li inserisco in un'array
            //che successivamente servirà per aggiungere gli elementi alla toolbar
            List<string> elementsToAdd = new List<string>();
            string[] folderEntries = Directory.GetDirectories(AttFolder);
            foreach (string folderName in folderEntries)
            {
                elementsToAdd.Add("D"+folderName);
            }

            string[] fileEntries = Directory.GetFiles(AttFolder);

            foreach (string fileName in fileEntries)
            {
                if (!((File.GetAttributes(fileName) & FileAttributes.Hidden) == FileAttributes.Hidden) || Load.ShowHidden)
                {
                    elementsToAdd.Add("F" + fileName);
                }
            }
            //Ordinamento elementi
            //Cerco se esiste il file che definisce gli ordinamenti
            //Se non lo trovo ordino alfabeticamente
            try
            {
                IsolatedStorageFileStream userConfigFile =
                    new IsolatedStorageFileStream(UserConfigFile, FileMode.Open);
                StreamReader readStream = new StreamReader(userConfigFile);
                string AttSavedPosition;
                int PositionAtt = 0;
                string TypeFound = "";
                while ((AttSavedPosition = readStream.ReadLine()) != null)
                {
                    int ItemIdx = elementsToAdd.IndexOf("D"+AttSavedPosition);
                    TypeFound = "D";
                    if (ItemIdx == -1)
                    {
                        ItemIdx = elementsToAdd.IndexOf("F" + AttSavedPosition);
                        TypeFound = "F";
                    }
                    if (ItemIdx!=-1){
                        //Elemento trovato verifico la posizione ed eventualmente riordino
                        elementsToAdd.RemoveAt(ItemIdx);
                        elementsToAdd.Insert(PositionAtt, TypeFound+AttSavedPosition);
                        PositionAtt++;
                    }
                }
                readStream.Close();
                userConfigFile.Close();
            }
            catch (System.IO.FileNotFoundException)
            {
                elementsToAdd.Sort();
            }
            //Analizzo il contenuto della directory, se rilevo un altra directory
            //aggiungo un bottone per aprirla, tooltip nome directory
            //Se rilevo un link, lo apro e collezione le informazioni per
            //aggiungere un bottone per lanciare l'applicazione
            //tooltip commento del link
            Font MeasureFont = this.MainToolbar.Font;
            foreach (string attElement in elementsToAdd)
            {
                string attElementType = attElement.Substring(0, 1);
                string attElementPath = attElement.Remove(0, 1);
                ToolStripButton AttBtn = new ToolStripButton();
                AttBtn.Alignment = Alignment;
                //le immagini sono già della dimensione giusta: nessun ridimensionamento automatico
                AttBtn.ImageScaling = ToolStripItemImageScaling.None;
                if (Load.ShowText)
                {
                    AttBtn.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    if (Load.LargeIcons)
                    {
                        AttBtn.TextImageRelation = TextImageRelation.ImageAboveText;
                    }
                    else
                    {
                        AttBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
                    }
                }
                else
                {
                    AttBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                }
                //Gestione rendering del bottone per gestire caso immagine+testo
                string BtnString = Path.GetFileNameWithoutExtension(attElementPath);
                if (String.IsNullOrEmpty(BtnString))
                {
                    BtnString = attElementPath;
                }
                string OrigString = BtnString;
                //il testo viene misurato con il carattere effettivo della toolbar
                int MaxTextWidth = 0;
                if (Load.LargeIcons)
                {
                    MaxTextWidth = this.Scaled(73);
                }
                else if (Load.TopOrBottom)
                {
                    MaxTextWidth = this.Scaled(138);
                }
                if (MaxTextWidth > 0)
                {
                    int CutIndex = 1;
                    while (CutIndex < OrigString.Length && TextRenderer.MeasureText(BtnString, MeasureFont).Width > MaxTextWidth)
                    {
                        BtnString = OrigString.Substring(0, OrigString.Length - CutIndex) + "...";
                        CutIndex++;
                    }
                }
                AttBtn.Text = BtnString;
                AttBtn.ToolTipText = OrigString;
                AttBtn.Tag = attElementPath;
                AttBtn.AllowDrop = true;
                //Gestione eventi click e drag and drop
                AttBtn.Click += new EventHandler(this.ToolStripButton_Click);
                AttBtn.DragEnter += new System.Windows.Forms.DragEventHandler(this.ToolStripButton_DragEnter);
                AttBtn.DragDrop += new System.Windows.Forms.DragEventHandler(this.ToolStripButton_DragDrop);
                AttBtn.MouseEnter += new System.EventHandler(this.MainForm_MouseEnter);
                AttBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToolStripButton_MouseDown);
                try
                {
                    AttBtn.Image = this.LoadItemBitmap(attElementPath, attElementType == "D", IconSize);
                }
                catch
                {
                    AttBtn.Image = null;
                }
                if (AttBtn.Image == null)
                {
                    NoError = false;
                    ErrorMessages += String.Format(attElementType == "D" ? ResourceMessages.ErrorFolderInclusion : ResourceMessages.ErrorFileInclusion, attElementPath);
                }
                Target.Add(AttBtn);
                this._ItemFolder[AttBtn] = AttFolder;
            }

            if (Load.FolderWatcher)
            {
                CreateFolderWatcher(AttFolder);
            }
            return NoError;
        }

        private void ToolStripButton_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void ToolStripButton_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string ParametersList = "";

            foreach (string file in files)
            {
                ParametersList += "\""+ file + "\" ";
            }
            if (sender is ToolStripButton)
            {
                ToolStripButton AttBtn = (ToolStripButton)sender;

                bool IsFolder = Directory.Exists(AttBtn.Tag.ToString());
                string DestinationDir = AttBtn.Tag.ToString();
                if (!IsFolder)
                {

                ShellLink link = new ShellLink();
                    try
                    {
                        link.Open(AttBtn.Tag.ToString());
                        IsFolder = Directory.Exists(link.Target);
                        DestinationDir = link.Target.ToString();
                        link.Dispose();

                    }
                    catch
                    {
                        IsFolder = false;
                    }
                }
                if (IsFolder)
                {
                    this._ParamList = (string[])e.Data.GetData(DataFormats.FileDrop);
                    this._DestinationPath = DestinationDir;
                    this.ContextMenuFolders.Show(this, MousePosition.X, MousePosition.Y );
                }
                else
                {
                    StartApplicationFromButton(AttBtn.Tag.ToString(), ParametersList);
                }
                AttBtn = null;
            }
            e.Effect = DragDropEffects.None;
        }

        void ToolStripButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripButton)
            {
                ToolStripButton AttBtn = (ToolStripButton)sender;
                StartApplicationFromButton(AttBtn.Tag.ToString());
                AttBtn = null;
            }
        }

        void ToolStripLabel_DoubleClick(object sender, EventArgs e)
        {
            if (this._OldMilliseconds != 0 && System.DateTime.Now.Millisecond - this._OldMilliseconds < 100)
            {
                this._OldMilliseconds = 0;
                if (sender is ToolStripLabel)
                {
                    ToolStripLabel AttLbl = (ToolStripLabel)sender;
                    StartApplicationFromButton(AttLbl.Tag.ToString());
                    AttLbl = null;
                }
            }
            else
            {
                this._OldMilliseconds = System.DateTime.Now.Millisecond;
            }
        }

        void StartApplicationFromButton(string AppPath)
        {
            //Il collegamento (.lnk) viene aperto direttamente dalla shell di Windows.
            //In precedenza veniva prima "risolto" con IShellLink.Resolve e un timeout di 1 ms:
            //sulle cartelle di rete (I:\Temp, \\server\share\temp) la risoluzione andava in
            //timeout (0x800705B4) e l'eccezione non gestita bloccava l'applicazione.
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(AppPath);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show(String.Format(ResourceMessages.InvalidFile), ResourceMessages.GenericErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void StartApplicationFromButton(string AppPath, string Parameters)
        {
            bool OkRun;
            OkRun = File.Exists(AppPath);
            if (OkRun && Path.GetExtension(AppPath).ToUpper() == ".LNK")
            {
                ShellLink link = new ShellLink();
                try
                {
                    link.Open(AppPath);
                    OkRun = File.Exists(link.Target);
                }
                catch
                {
                    OkRun = false;
                }
                finally
                {
                    link.Dispose();
                }
            }
            if (OkRun)
            {

                Process.Start(AppPath, Parameters);
            }
            else
            {
                MessageBox.Show(String.Format(ResourceMessages.InvalidFile), ResourceMessages.GenericErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void MainForm_MouseEnter(object sender, EventArgs e)
        {
            if (this._AutoHide)
            {
                if (this.ToolbarIsHide)
                {
                    this.timerAutoHide.Enabled = true;
                }
            }
            else
            {
                this.Activate();
            }
        }

        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void impostazioniToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings MySettings = new FormSettings();
            DialogResult Result = MySettings.ShowDialog();
            //Cambio di lingua nelle impostazioni: la finestra viene ricreata nella nuova
            //lingua (con i valori non ancora salvati), così anche il layout specifico
            //della lingua e lo zoom (DPI) sono applicati correttamente.
            while (Result == DialogResult.Retry)
            {
                FormSettings.SettingsDraft Draft = MySettings.Draft;
                MySettings.Dispose();
                MySettings = new FormSettings(Draft);
                Result = MySettings.ShowDialog();
            }
            MySettings.Dispose();
            if (Result != DialogResult.OK)
            {
                //lingua eventualmente cambiata ma non salvata: torno a quella impostata
                LanguageManager.ChangeLanguage(this);
            }
            if (Result == DialogResult.OK)
            {
                if (Program.bRebootNeeded)
                {
                    MySettings = null;
                    this.Close();
                }
                else
                {
                    this.RefreshToolbar(true);
                    MySettings = null;
                    this._RightClickFired = false;
                    if (this._AutoHide)
                    {
                        this.timerAutoHide.Interval = 1000;
                        this.timerAutoHide.Enabled = true;
                    }
                }
            }
            
        }

        private void copiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string FileDest;
            bool StopCopy = false ;
            bool CopyFileOk;
            foreach (string file in this._ParamList)
            {
                FileDest = Path.Combine(this._DestinationPath, Path.GetFileName(file));
                CopyFileOk = !File.Exists(FileDest);
                if (!CopyFileOk )
                {
                    switch (MessageBox.Show(String.Format(ResourceMessages.AskOverwriteFile, FileDest), ResourceMessages.FileCopyTitle , MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            CopyFileOk = true;
                            break;
                        case DialogResult.No:
                            break;
                        case DialogResult.Cancel:
                            StopCopy = true;
                            break;
                    }
                }
                if (CopyFileOk)
                {
                    try
                    {
                        File.Copy( file, FileDest, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (StopCopy)
                {
                    break;
                }
            }
        }

        private void spostaToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string FileDest;
            bool StopCopy = false;
            bool CopyFileOk;
            foreach (string file in this._ParamList)
            {
                FileDest =  Path.Combine(this._DestinationPath, Path.GetFileName(file));
                CopyFileOk = !File.Exists(FileDest);
                if (!CopyFileOk)
                {
                    switch (MessageBox.Show(String.Format(ResourceMessages.AskOverwriteFile, FileDest), ResourceMessages.FileCopyTitle, MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Yes:
                            CopyFileOk = true;
                            break;
                        case DialogResult.No:
                            break;
                        case DialogResult.Cancel:
                            StopCopy = true;
                            break;
                    }
                }
                if (CopyFileOk)
                {
                    try
                    {
                        if (File.Exists(FileDest))
                        {
                            File.Delete(FileDest);
                        }
                        File.Move(file, FileDest);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                if (StopCopy)
                {
                    break;
                }
            }
        }

        private void informazionisuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox AboutBoxWnd = new AboutBox();
            AboutBoxWnd.ShowDialog();
            this._RightClickFired = false;
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        private void ToolStripButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is ToolStripButton && e.Button == MouseButtons.Right)
            {
                ToolStripButton AttBtn = (ToolStripButton)sender;
                ShellContextMenu scm = new ShellContextMenu();
                FileInfo[] files = new FileInfo[1];
                files[0] = new FileInfo(AttBtn.Tag.ToString());
                Point ShowPos = Cursor.Position;
                bool AlignRight = false;
                bool AlignBottom = false;
                //Il menu si apre sul lato interno della barra e si "allontana" dal bordo,
                //così resta sullo stesso monitor della barra (ticket #24: con la barra sul
                //bordo destro del monitor centrale il menu finiva sul monitor di destra).
                //Per gli elementi dell'elenco a comparsa il menu si apre dove si trova il mouse.
                switch (AttBtn.Owner == this.MainToolbar ? this.Edge : AppBarEdges.Float)
                {
                    case AppBarEdges.Left:
                        ShowPos.X = this.Bounds.Right;
                        break;
                    case AppBarEdges.Right:
                        ShowPos.X = this.Bounds.Left;
                        AlignRight = true;
                        break;
                    case AppBarEdges.Top:
                        ShowPos.Y = this.Bounds.Bottom;
                        break;
                    case AppBarEdges.Bottom:
                        ShowPos.Y = this.Bounds.Top;
                        AlignBottom = true;
                        break;
                }

                scm.ShowContextMenu(files, ShowPos, AlignRight, AlignBottom);
                AttBtn = null;
                scm = null;
            }
        }

        private void aggiornaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.RefreshToolbar(true);
            this._RightClickFired = false;
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            this.timerRefresh.Enabled = false;
            this.RefreshToolbar();
        }

        private void SetTimerOn()
        {
            this.timerRefresh.Interval = 1;
            this.timerRefresh.Enabled = true;
        }

        private void ContextMenuForm_Opening(object sender, CancelEventArgs e)
        {
            this._RightClickFired = true;
            this.timerAutoHide.Enabled = false;
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            this.mostraTestoToolStripMenuItem.Checked = MySettings.ShowText;
            this.mostratitoloToolStripMenuItem.Checked = MySettings.FolderCaption;
            this.nascondiautomaticamenteToolStripMenuItem.Checked = MySettings.AutoHide;
            MySettings = null;
            LanguageManager.ChangeContextMenuLang(sender);
        }

        private void ContextMenuFolders_Opening(object sender, CancelEventArgs e)
        {
            LanguageManager.ChangeContextMenuLang(sender);
        }

        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            if (this._AutoHide && !this.ToolbarIsHide)
            {
                this.timerAutoHide.Enabled = true;
            }
        }

        private void timerAutoHide_Tick(object sender, EventArgs e)
        {
            if (this._PopupOpen)
            {
                //finché l'elenco a comparsa è aperto la toolbar non si nasconde
                this.timerAutoHide.Enabled = false;
                return;
            }
            if (!this._RightClickFired)
            {
                if (this.ToolbarIsHide)
                {
                    SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
                    MySettings.Reload();
                    int StandardHeightWidth = this.Scaled(MySettings.ToolbarHeightWidth);
                    if (this.Edge == AppBarEdges.Top || this.Edge == AppBarEdges.Bottom)
                    {
                        this.StandardHeight = StandardHeightWidth;
                    }
                    else
                    {
                        this.StandardWidth = StandardHeightWidth;
                    }
                    MySettings = null;
                }
                else
                {
                    //quando la toolbar si nasconde lo scorrimento torna all'inizio
                    if (this._ScrollOffset != 0)
                    {
                        this._ScrollOffset = 0;
                        this.ApplyScroll();
                    }
                    if (this.Edge == AppBarEdges.Top || this.Edge == AppBarEdges.Bottom)
                    {
                        this.StandardHeight = Math.Max(4, this.Scaled(4));
                    }
                    else
                    {
                        this.StandardWidth = Math.Max(4, this.Scaled(4));
                    }
                }
                this.ToolbarIsHide = !this.ToolbarIsHide;
                this.timerAutoHide.Enabled = false;
                this.timerAutoHide.Interval = this._AutoHideInterval;
            }
        }

        private void MainToolbar_MouseEnter(object sender, EventArgs e)
        {
            this.timerAutoHide.Enabled = false;
            if (this._AutoHide)
            {
                if (this.ToolbarIsHide)
                {
                    this.timerAutoHide.Enabled = true;
                }
            }
            else
            {
                this.Activate();
            }
        }

        private void MainToolbar_MouseLeave(object sender, EventArgs e)
        {
            if (this._AutoHide && !this.ToolbarIsHide)
            {
                this.timerAutoHide.Enabled = true;
            }
        }

        private void ContextMenuForm_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            
        }

        private void mostraTestoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            MySettings.ShowText = !this.mostraTestoToolStripMenuItem.Checked;
            MySettings.Save();
            MySettings = null;
            this.RefreshToolbar(true);
            this._RightClickFired = false;
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        private void mostratitoloToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            MySettings.FolderCaption = !this.mostratitoloToolStripMenuItem.Checked;
            MySettings.Save();
            MySettings = null;
            this.RefreshToolbar(true);
            this._RightClickFired = false;
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        private void nascondiautomaticamenteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            MySettings.AutoHide = !this.nascondiautomaticamenteToolStripMenuItem.Checked;
            MySettings.Save();
            MySettings = null;
            this.RefreshToolbar(true);
            this._RightClickFired = false;
            if (this._AutoHide)
            {
                this.timerAutoHide.Interval = 1000;
                this.timerAutoHide.Enabled = true;
            }
        }

        private void OrderModetoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.OrderModetoolStripMenuItem.Checked)
            {
                //Esco dalla modalità ordinamento, salvo e reimposto proprietà toolbar

                IsolatedStorageFileStream userConfigFile =
                new IsolatedStorageFileStream(UserConfigFile, FileMode.Create);

                // create a writer to the stream...
                StreamWriter writeStream = new StreamWriter(userConfigFile);

                //ordine di visualizzazione (le cartelle allineate alla fine sono
                //memorizzate nella toolbar in ordine inverso)
                foreach (ToolStripItem AttBtn in this.DisplayOrderItems())
                {
                    if (AttBtn is ToolStripButton && !(AttBtn is ToolbarControlButton))
                    {
                        writeStream.WriteLine(AttBtn.Tag);
                    }
                }
                // clean up...
                writeStream.Flush();
                writeStream.Close();
                userConfigFile.Close();
                this.MainToolbar.AllowItemReorder = false;
                this.MainToolbar.AllowDrop = true;
            }
            else
            {
                //Entro dalla modalità ordinamento, reimposto proprietà toolbar
                this.MainToolbar.AllowDrop = false;
                this.MainToolbar.AllowItemReorder = true;
            }
            this.OrderModetoolStripMenuItem.Checked = !this.OrderModetoolStripMenuItem.Checked;
        }

        private void MainForm_DoubleClick(object sender, EventArgs e)
        {

        }

    }

    /// <summary>
    /// Disegno personalizzato di icona + testo. Le impostazioni (testo, icone grandi)
    /// possono essere diverse per ogni cartella, quindi si decide elemento per elemento:
    /// testo sotto l'icona (icone grandi) o accanto (icone piccole).
    /// </summary>
    public class MyRenderer : ToolStripProfessionalRenderer
    {
        public MyRenderer()
        {
        }

        private static bool IsCustomItem(ToolStripItem Item)
        {
            return Item is ToolStripButton && !(Item is ToolbarControlButton)
                && Item.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText;
        }

        private static bool IsLarge(ToolStripItem Item)
        {
            return Item.TextImageRelation == TextImageRelation.ImageAboveText;
        }

        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            int x;
            int y;
            if (IsCustomItem(e.Item) && e.Image != null)
            {
                if (IsLarge(e.Item))
                {
                    x = e.Item.Width / 2 - e.Image.Width / 2;
                    y = 0;
                }
                else
                {
                    x = 0;
                    y = e.Item.Height / 2 - e.Image.Height / 2;
                }
                e.Graphics.DrawImage(e.Image, new Point(x, y));
            }
            else
            {
                base.OnRenderItemImage(e);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (IsCustomItem(e.Item))
            {
                using (SolidBrush TextBrush = new SolidBrush(e.Item.ForeColor))
                {
                    if (IsLarge(e.Item))
                    {
                        e.Graphics.DrawString(e.Item.Text, e.Item.Font, TextBrush, 0, e.Item.Height - TextRenderer.MeasureText(e.Item.Text, e.Item.Font).Height-3 );
                    }
                    else
                    {
                        int ImageWidth = e.Item.Image != null ? e.Item.Image.Width : 0;
                        e.Graphics.DrawString(e.Item.Text, e.Item.Font, TextBrush, ImageWidth, e.Item.Height / 2 - TextRenderer.MeasureText(e.Item.Text, e.Item.Font).Height/2);
                    }
                }
            }
            else
            {
                base.OnRenderItemText(e);
            }

        }
    }
}
