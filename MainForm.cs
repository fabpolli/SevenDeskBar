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

        delegate void DisposeToolbarObjectsCallback();

        public MainForm()
        {
            LanguageManager.ChangeLanguage(this);
            InitializeComponent();
            ShellApi.SetWindowLong(this.Handle, ShellApi.GWL_EXSTYLE,
                    ShellApi.GetWindowLong(this.Handle, ShellApi.GWL_EXSTYLE) |
                            ShellApi.WS_EX_TOOLWINDOW);

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
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

        }

        private void watcher_FolderChange(object sender, FileSystemEventArgs e)
        {
            if (sender is FileSystemWatcher)
            {
                FileSystemWatcher AttWatch = (FileSystemWatcher)sender;
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
                this.MainToolbar.Items[0].Dispose();
            }
        }

        private void RefreshToolbar()
        {
            this.RefreshToolbar(false);
        }


        private void RefreshToolbar(bool ReEdge)
        {
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


        private void LoadObjects(bool OnLoadingApp)
        {
            this.SuspendLayout();
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            string FoldersToAnalize = MySettings.IncludedFolders;
            int StandardHeightWidth = MySettings.ToolbarHeightWidth;
            string[] FolderArray = FoldersToAnalize.Split('|');
            bool LargeIcons = MySettings.LargeIcons;
            bool NoError = true;
            string ErrorMessages = "";
            this._AutoHide = MySettings.AutoHide;
            this._AutoHideInterval = MySettings.AutoHideInterval;
            this.ToolbarIsHide = !this._AutoHide;
            bool EnableGlass = MySettings.GlassStyleEffect;
            bool ShowHidden = MySettings.ShowHidden;
            if (LargeIcons)
            {
                this.MainToolbar.ImageScalingSize = new System.Drawing.Size(32, 32);
            }
            else
            {
                this.MainToolbar.ImageScalingSize = new System.Drawing.Size(16, 16);
            }
            Array AppBarEdgesValues = Enum.GetValues(typeof(AppBarEdges));
            AppBarEdges AttAppBarEdge = (AppBarEdges)AppBarEdgesValues.GetValue(Convert.ToInt32(MySettings.TollbarDockPosition));
            bool TopOrBottom = AttAppBarEdge == AppBarEdges.Top || AttAppBarEdge == AppBarEdges.Bottom;

            if (MySettings.ShowText)
            {
                this.MainToolbar.Renderer = new MyRenderer(LargeIcons, TopOrBottom);
            }
            else
            {
                this.MainToolbar.RenderMode = ToolStripRenderMode.ManagerRenderMode;
            }

            foreach (string AttFolder in FolderArray)
            {
                if (!String.IsNullOrEmpty(AttFolder) && Directory.Exists(AttFolder))
                {
                    //Aggiungo controlli alla toolbar in base alle cartelle
                    //incluse
                    NoError = NoError && AnalyzeFolder(AttFolder, MySettings.FolderCaption, ref ErrorMessages, AttAppBarEdge, MySettings.FolderActiveControl, LargeIcons, MySettings.ShowText, TopOrBottom, EnableGlass, ShowHidden);
                }
            }
            
            if (!OnLoadingApp)
            {
                this.Edge = AttAppBarEdge;
            }
            if (TopOrBottom)
            {
                this.StandardHeight = StandardHeightWidth;
                this.MainToolbar.MaximumSize = new System.Drawing.Size(Screen.GetWorkingArea(this).Width, this.StandardHeight);
                this.MainToolbar.MinimumSize = new System.Drawing.Size(Screen.GetWorkingArea(this).Width, this.StandardHeight);
                this.MainToolbar.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            }
            else
            {
                this.StandardWidth = StandardHeightWidth;
                this.MainToolbar.MaximumSize = new System.Drawing.Size(this.StandardWidth, Screen.GetWorkingArea(this).Height);
                this.MainToolbar.MinimumSize = new System.Drawing.Size(this.StandardWidth, Screen.GetWorkingArea(this).Height);
                this.MainToolbar.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
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
                Graphics g = this.CreateGraphics();
                Rectangle glassRect = new Rectangle(0, 0, this.Width, this.Height);
                Region rgn = new Region(glassRect);
                VistaApi.DWM_BLURBEHIND bb = new VistaApi.DWM_BLURBEHIND();
                bb.dwFlags = (VistaApi.DWM_BB_ENABLE | VistaApi.DWM_BB_BLURREGION);
                bb.fEnable = true;
                bb.hRgnBlur = rgn.GetHrgn(g);
                // '#Enable blur-behind effect
                VistaApi.DwmEnableBlurBehindWindow(this.Handle, ref bb);
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                if (!this._OldBackColor.IsEmpty)
                {
                    this.BackColor = this._OldBackColor;
                    this.MainToolbar.BackColor = this._OldToolbarBackColor;
                }
            }
            this.ResumeLayout();
            if (!NoError)
            {
                MessageBox.Show(ErrorMessages, ResourceMessages.ErrorToolbarCreation, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool AnalyzeFolder(string AttFolder, bool SetCaption, ref string ErrorMessages, AppBarEdges AttEdge, bool FolderWatcher, bool LargeIcons, bool ShowText, bool TopOrBottom, bool EnableGlass, bool ShowHiddenFiles)
        {
            bool NoError = true;
            if (this.MainToolbar.Items.Count > 0)
            {
                ToolStripSeparator AttSep = new ToolStripSeparator();
                this.MainToolbar.Items.Add(AttSep);
            }

            if (SetCaption)
            {
                ToolStripLabel AttLbl = new ToolStripLabel();
                AttLbl.Text = Path.GetFileName(AttFolder).ToString();
                AttLbl.TextAlign = ContentAlignment.MiddleLeft;
                AttLbl.ToolTipText = AttLbl.Text;
                AttLbl.Tag = AttFolder;
                AttLbl.Click += new EventHandler(this.ToolStripLabel_DoubleClick);
                if (EnableGlass)
                {
                    AttLbl.ForeColor = Color.White;
                }
                //switch (AttEdge)
                //{
                //    case AppBarEdges.Top:
                //        AttLbl.TextAlign = ContentAlignment.MiddleCenter;
                //        break;
                //    case AppBarEdges.Bottom:
                //        AttLbl.TextAlign = ContentAlignment.MiddleCenter;
                //        break;
                //    case AppBarEdges.Left:
                //        AttLbl.TextAlign = ContentAlignment.MiddleLeft;
                //        break;
                //    case AppBarEdges.Right:
                //        AttLbl.TextAlign = ContentAlignment.MiddleRight;
                //        break;
                //}
                this.MainToolbar.Items.Add(AttLbl);
                AttLbl = null;
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
                if (!((File.GetAttributes(fileName) & FileAttributes.Hidden) == FileAttributes.Hidden) || ShowHiddenFiles)
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
            foreach (string attElement in elementsToAdd)
            {
                string attElementType = attElement.Substring(0, 1);
                string attElementPath = attElement.Remove(0, 1);
                ToolStripButton AttBtn = new ToolStripButton();
                if (ShowText)
                {
                    AttBtn.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    if (LargeIcons)
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
                string OrigString = BtnString;
                if (LargeIcons)
                {
                    int CutIndex = 1;
                    
                    while (TextRenderer.MeasureText(BtnString, AttBtn.Font).Width > AttBtn.Width + 50)
                    {
                        BtnString = OrigString.Substring(0, OrigString.Length - CutIndex) + "...";
                        CutIndex++;
                    }
                }
                else
                {
                    if (TopOrBottom)
                    {
                        int CutIndex = 1;
                        while (TextRenderer.MeasureText(BtnString, AttBtn.Font).Width > AttBtn.Width + 115)
                        {
                            BtnString = OrigString.Substring(0, OrigString.Length - CutIndex) + "...";
                            CutIndex++;
                        }
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
                if (attElementType == "D")
                {
                    //Directory
                    try
                    {
                        Icon FileIcon;
                        if (LargeIcons)
                        {
                            FileIcon = ShellIcon.GetLargeIcon(attElementPath);
                        }
                        else
                        {
                            FileIcon = ShellIcon.GetSmallIcon(attElementPath);
                        }
                        AttBtn.Image = FileIcon.ToBitmap();
                        FileIcon.Dispose();
                    }
                    catch
                    {
                        NoError = false;
                        ErrorMessages += String.Format(ResourceMessages.ErrorFolderInclusion, attElementPath);
                    }
                    this.MainToolbar.Items.Add(AttBtn);
                    AttBtn = null;
                }
                else
                {
                    //File
                    if (Path.GetExtension(attElementPath).ToUpper() == ".LNK")
                    {
                        ShellLink link = new ShellLink();
                        try
                        {
                            link.Open(attElementPath);
                            Icon FileIcon;
                            if (LargeIcons)
                            {
                                FileIcon = link.LargeIcon;
                            }
                            else
                            {
                                FileIcon = link.SmallIcon;
                            }

                            AttBtn.Image = FileIcon.ToBitmap();
                            link.Dispose();
                            FileIcon.Dispose();
                        }
                        catch
                        {
                            //Provo a esrarre l'icona dall'exe
                            try
                            {
                                Icon FileIcon = Icon.ExtractAssociatedIcon(attElementPath);
                                AttBtn.Image = FileIcon.ToBitmap();
                                FileIcon.Dispose();
                            }
                            catch
                            {
                                //provo a estrarre l'icona dalla shell
                                try
                                {
                                    Icon FileIcon;
                                    if (LargeIcons)
                                    {
                                        FileIcon = ShellIcon.GetLargeIcon(attElementPath);
                                    }
                                    else
                                    {
                                        FileIcon = ShellIcon.GetSmallIcon(attElementPath);
                                    }
                                    AttBtn.Image = FileIcon.ToBitmap();
                                    FileIcon.Dispose();
                                }
                                catch
                                {
                                    NoError = false;
                                    ErrorMessages += String.Format(ResourceMessages.ErrorFileInclusion, attElementPath);
                                }
                            }

                        }
                    }
                    else if (Path.GetExtension(attElementPath).ToUpper() == ".EXE")
                    {
                        //Recupero icona e informazioni del file eseguibile
                        Icon FileIcon = Icon.ExtractAssociatedIcon(attElementPath);
                        AttBtn.Image = FileIcon.ToBitmap();
                        FileIcon.Dispose();
                    }
                    else
                    {
                        //File generico, recupero icona dalla shell
                        Icon FileIcon;
                        if (LargeIcons)
                        {
                            FileIcon = ShellIcon.GetLargeIcon(attElementPath);
                        }
                        else
                        {
                            FileIcon = ShellIcon.GetSmallIcon(attElementPath);
                        }
                        AttBtn.Image = FileIcon.ToBitmap();
                        FileIcon.Dispose();
                    }
                    this.MainToolbar.Items.Add(AttBtn);
                    AttBtn = null;
                }
            }

            if (FolderWatcher)
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
            if (Path.GetExtension(AppPath).ToUpper() == ".LNK")
            {
                    ShellLink link = new ShellLink();
                    try
                    {
                        link.Open(AppPath);
                    }
                    finally
                    {
                        link.Dispose();
                    }
            }
            try
            {
                Process.Start(AppPath);
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
            if (MySettings.ShowDialog() == DialogResult.OK)
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
                switch (this.Edge)
                {
                    case AppBarEdges.Left:
                        ShowPos.X = this.Width;
                        break;
                    case AppBarEdges.Right:
                        ShowPos.X = this.Left;
                        break;
                    case AppBarEdges.Top:
                        ShowPos.Y = this.Height ;
                        break;
                    case AppBarEdges.Bottom:
                        ShowPos.Y = this.Top;
                        break;
                }
                
                scm.ShowContextMenu(files, ShowPos);
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
            
            if (!this._RightClickFired)
            {
                if (this.ToolbarIsHide)
                {
                    SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
                    MySettings.Reload();
                    int StandardHeightWidth = MySettings.ToolbarHeightWidth;
                    if (this.Edge == AppBarEdges.Top || this.Edge == AppBarEdges.Bottom)
                    {
                        this.StandardHeight = StandardHeightWidth + 4;
                        this.MainToolbar.Height = 34;
                    }
                    else
                    {
                        this.StandardWidth = StandardHeightWidth;
                    }
                    MySettings = null;
                }
                else
                {
                    if (this.Edge == AppBarEdges.Top || this.Edge == AppBarEdges.Bottom)
                    {
                        this.StandardHeight = 4;
                    }
                    else
                    {
                        this.StandardWidth = 4;
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

                foreach (Object AttBtn in this.MainToolbar.Items)
                {
                    if (AttBtn is ToolStripButton)
                    {
                        ToolStripButton TmpBtn = (ToolStripButton)AttBtn;
                        writeStream.WriteLine(TmpBtn.Tag);
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

    public class MyRenderer : ToolStripProfessionalRenderer
    {

        bool _LargeIcon;
        bool _TopOrBottom;

        public MyRenderer()
        {
        }

        public MyRenderer(bool LargeIcon, bool TopOrBottom)
        {
            this._LargeIcon = LargeIcon;
            this._TopOrBottom = TopOrBottom;
        }


        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            int x;
            int y;
            if (e.Item is ToolStripButton)
            {
                if (this._LargeIcon)
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
            if (e.Item is ToolStripButton)
            {
                if (this._LargeIcon)
                {
                    e.Graphics.DrawString(e.Item.Text, e.Item.Font, new SolidBrush(e.Item.ForeColor), 0, e.Item.Height - TextRenderer.MeasureText(e.Item.Text, e.Item.Font).Height-3 );
                }
                else
                {
                    e.Graphics.DrawString(e.Item.Text, e.Item.Font, new SolidBrush(e.Item.ForeColor), e.Item.Image.Width, e.Item.Height / 2 - TextRenderer.MeasureText(e.Item.Text, e.Item.Font).Height/2);
                }
            }
            else
            {
                base.OnRenderItemText(e);
            }

        }
    }
}
