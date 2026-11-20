using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SevenDeskBar
{
    /// <summary>
    /// Ticket #22: trascinando file o cartelle su un punto libero della toolbar (non sopra
    /// un'icona) viene creato un collegamento nella cartella della sezione più vicina,
    /// come facevano le barre degli strumenti di Windows XP.
    /// Il trascinamento sopra un'icona mantiene il comportamento precedente
    /// (copia/sposta sulle cartelle, apertura con il programma sugli eseguibili).
    /// </summary>
    public partial class MainForm
    {
        private void InitDragDropHandling()
        {
            this.MainToolbar.DragEnter += new DragEventHandler(this.MainToolbar_DragOver);
            this.MainToolbar.DragOver += new DragEventHandler(this.MainToolbar_DragOver);
            this.MainToolbar.DragDrop += new DragEventHandler(this.MainToolbar_DragDrop);
        }

        private bool CanCreateShortcutFromDrag(DragEventArgs e)
        {
            //in modalità ordinamento il trascinamento serve a spostare le icone
            return !this.MainToolbar.AllowItemReorder
                && e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop, false);
        }

        private void MainToolbar_DragOver(object sender, DragEventArgs e)
        {
            if (!this.CanCreateShortcutFromDrag(e))
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = e.AllowedEffect & DragDropEffects.Copy;
            }
        }

        private void MainToolbar_DragDrop(object sender, DragEventArgs e)
        {
            if (!this.CanCreateShortcutFromDrag(e))
            {
                return;
            }
            string[] Files = e.Data.GetData(DataFormats.FileDrop) as string[];
            string Folder = this.FolderAtPoint(this.MainToolbar.PointToClient(new Point(e.X, e.Y)));
            if (Files == null || Folder == null)
            {
                return;
            }
            bool Created = false;
            foreach (string AttFile in Files)
            {
                try
                {
                    Created = this.CreateShortcut(AttFile, Folder) || Created;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format(ResourceMessages.ResourceManager.GetString("ErrorCreateShortcut") ?? "{0} -> {1}", AttFile, Folder)
                        + Environment.NewLine + ex.Message,
                        ResourceMessages.GenericErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            if (Created)
            {
                //aggiornamento al termine dell'operazione di trascinamento
                this.BeginInvoke(new MethodInvoker(delegate { this.RefreshToolbar(); }));
            }
        }

        /// <summary>
        /// Cartella della sezione della toolbar più vicina al punto (coordinate della toolbar)
        /// </summary>
        private string FolderAtPoint(Point ClientPoint)
        {
            bool Vertical = this.MainToolbar.LayoutStyle == ToolStripLayoutStyle.VerticalStackWithOverflow;
            string Best = null;
            int BestDistance = int.MaxValue;
            foreach (ToolStripItem AttItem in this.DisplayOrderItems())
            {
                string ItemFolder;
                if (!AttItem.Available || AttItem.Placement != ToolStripItemPlacement.Main
                    || !this._ItemFolder.TryGetValue(AttItem, out ItemFolder))
                {
                    continue;
                }
                Rectangle b = AttItem.Bounds;
                int Distance;
                if (Vertical)
                {
                    Distance = ClientPoint.Y < b.Top ? b.Top - ClientPoint.Y : (ClientPoint.Y > b.Bottom ? ClientPoint.Y - b.Bottom : 0);
                }
                else
                {
                    Distance = ClientPoint.X < b.Left ? b.Left - ClientPoint.X : (ClientPoint.X > b.Right ? ClientPoint.X - b.Right : 0);
                }
                if (Distance < BestDistance)
                {
                    BestDistance = Distance;
                    Best = ItemFolder;
                }
            }
            return Best;
        }

        private static string UniquePath(string Folder, string BaseName, string Extension)
        {
            string Candidate = Path.Combine(Folder, BaseName + Extension);
            int Counter = 2;
            while (File.Exists(Candidate) || Directory.Exists(Candidate))
            {
                Candidate = Path.Combine(Folder, BaseName + " (" + Counter + ")" + Extension);
                Counter++;
            }
            return Candidate;
        }

        /// <summary>
        /// Crea nella cartella un collegamento all'elemento trascinato.
        /// I collegamenti (.lnk, .url) trascinati vengono semplicemente copiati.
        /// </summary>
        private bool CreateShortcut(string Source, string Folder)
        {
            string Trimmed = Source.TrimEnd('\\');
            //elemento già presente nella cartella: niente da fare
            if (String.Equals(Path.GetDirectoryName(Trimmed), Folder.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            bool IsDirectory = Directory.Exists(Source);
            string Extension = IsDirectory ? "" : Path.GetExtension(Trimmed).ToLowerInvariant();
            if (Extension == ".lnk" || Extension == ".url")
            {
                string Copy = UniquePath(Folder, Path.GetFileNameWithoutExtension(Trimmed), Extension);
                File.Copy(Source, Copy);
                return true;
            }

            string BaseName = IsDirectory ? Path.GetFileName(Trimmed) : Path.GetFileNameWithoutExtension(Trimmed);
            if (String.IsNullOrEmpty(BaseName))
            {
                //radice di un disco (C:\) o di una condivisione (\\server\share)
                BaseName = Trimmed.Replace(":", "").Replace("\\", " ").Trim();
            }
            foreach (char Invalid in Path.GetInvalidFileNameChars())
            {
                BaseName = BaseName.Replace(Invalid, '_');
            }
            string LinkPath = UniquePath(Folder, BaseName, ".lnk");
            ShellLink Link = new ShellLink();
            try
            {
                Link.Target = Source;
                if (!IsDirectory)
                {
                    string Dir = Path.GetDirectoryName(Source);
                    if (!String.IsNullOrEmpty(Dir))
                    {
                        Link.WorkingDirectory = Dir;
                    }
                }
                Link.Save(LinkPath);
            }
            finally
            {
                Link.Dispose();
            }
            return true;
        }
    }
}
