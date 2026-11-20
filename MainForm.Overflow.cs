using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SevenDeskBar
{
    /// <summary>
    /// Pulsante "di servizio" della toolbar (scorrimento / altri elementi).
    /// Non rappresenta un file o una cartella: è escluso dal salvataggio dell'ordinamento
    /// e dal disegno personalizzato di MyRenderer.
    /// </summary>
    public class ToolbarControlButton : ToolStripButton
    {
    }

    /// <summary>
    /// Conversione da/verso stringa delle impostazioni di carattere e colore
    /// </summary>
    internal static class SettingsHelper
    {
        public static Font ParseFont(string Value)
        {
            if (String.IsNullOrEmpty(Value))
            {
                return null;
            }
            try
            {
                return (Font)TypeDescriptor.GetConverter(typeof(Font)).ConvertFromInvariantString(Value);
            }
            catch
            {
                return null;
            }
        }

        public static string FontToString(Font Value)
        {
            if (Value == null)
            {
                return "";
            }
            return TypeDescriptor.GetConverter(typeof(Font)).ConvertToInvariantString(Value);
        }

        public static Color ParseColor(string Value)
        {
            if (String.IsNullOrEmpty(Value))
            {
                return Color.Empty;
            }
            try
            {
                return ColorTranslator.FromHtml(Value);
            }
            catch
            {
                return Color.Empty;
            }
        }

        public static string ColorToString(Color Value)
        {
            if (Value.IsEmpty)
            {
                return "";
            }
            return ColorTranslator.ToHtml(Color.FromArgb(Value.ToArgb()));
        }
    }

    /// <summary>
    /// Gestione degli elementi che non entrano nella toolbar:
    ///  - modalità "elenco a comparsa": un pulsante » apre un elenco ordinato a griglia,
    ///    largo quanto la toolbar (o con più colonne) e scorrevole se troppo lungo;
    ///  - modalità "scorrimento": la toolbar stessa scorre con la rotella del mouse o con
    ///    i pulsanti freccia; quando la toolbar si nasconde automaticamente lo scorrimento
    ///    torna all'inizio.
    /// </summary>
    public partial class MainForm
    {
        private const int OverflowModePopup = 0;
        private const int OverflowModeScroll = 1;

        private int _OverflowMode = OverflowModePopup;
        private int _OverflowColumns = 1;
        private int _ScrollOffset = 0;
        private bool _PopupOpen = false;
        private DateTime _PopupClosedAt = DateTime.MinValue;
        private bool _UpdatingOverflowButtons = false;
        private ToolbarControlButton _BtnScrollBack;
        private ToolbarControlButton _BtnScrollForward;
        private ToolbarControlButton _BtnMore;
        private ToolStripDropDown _OverflowPopup;

        private void InitOverflowHandling()
        {
            //gli elementi in eccesso non finiscono più nel menu standard (disordinato) della ToolStrip
            this.MainToolbar.CanOverflow = false;
            this.MainToolbar.LayoutCompleted += new EventHandler(this.MainToolbar_LayoutCompleted);
            this.MainToolbar.MouseWheel += new MouseEventHandler(this.Toolbar_MouseWheel);
            this.MouseWheel += new MouseEventHandler(this.Toolbar_MouseWheel);
        }

        private ToolbarControlButton NewControlButton(string Glyph, string ToolTipKey, ToolStripItemAlignment Alignment)
        {
            ToolbarControlButton Btn = new ToolbarControlButton();
            Btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            Btn.Text = Glyph;
            Btn.AutoToolTip = false;
            Btn.ToolTipText = ResourceMessages.ResourceManager.GetString(ToolTipKey) ?? "";
            Btn.Alignment = Alignment;
            Btn.Overflow = ToolStripItemOverflow.Never;
            Btn.Available = false;
            Btn.MouseEnter += new EventHandler(this.MainForm_MouseEnter);
            return Btn;
        }

        /// <summary>
        /// Crea i pulsanti di servizio. Vanno in testa alla collezione: quando lo spazio
        /// non basta la ToolStrip scarta per primi gli ultimi elementi, quindi questi
        /// pulsanti restano sempre visibili (quelli allineati a destra compaiono in fondo).
        /// </summary>
        private void CreateOverflowButtons(bool TopOrBottom)
        {
            this.MainToolbar.CanOverflow = false;
            // ◄ ► ▲ ▼ sono presenti in tutti i font standard di Windows
            this._BtnScrollBack = this.NewControlButton(TopOrBottom ? "◄" : "▲", "ToolbarScrollBack", ToolStripItemAlignment.Left);
            this._BtnScrollForward = this.NewControlButton(TopOrBottom ? "►" : "▼", "ToolbarScrollForward", ToolStripItemAlignment.Right);
            this._BtnMore = this.NewControlButton("»", "ToolbarMoreItems", ToolStripItemAlignment.Right);
            this._BtnScrollBack.Click += new EventHandler(this.BtnScrollBack_Click);
            this._BtnScrollForward.Click += new EventHandler(this.BtnScrollForward_Click);
            this._BtnMore.Click += new EventHandler(this.BtnMore_Click);
            this.MainToolbar.Items.Insert(0, this._BtnMore);
            this.MainToolbar.Items.Insert(0, this._BtnScrollForward);
            this.MainToolbar.Items.Insert(0, this._BtnScrollBack);
        }

        /// <summary>
        /// Elementi delle cartelle nell'ordine in cui appaiono a video: prima quelli
        /// allineati all'inizio, poi quelli allineati alla fine (che nella collezione
        /// della ToolStrip sono memorizzati in ordine inverso).
        /// </summary>
        private List<ToolStripItem> DisplayOrderItems()
        {
            List<ToolStripItem> Start = new List<ToolStripItem>();
            List<ToolStripItem> End = new List<ToolStripItem>();
            foreach (ToolStripItem AttItem in this.MainToolbar.Items)
            {
                if (AttItem is ToolbarControlButton)
                {
                    continue;
                }
                if (AttItem.Alignment == ToolStripItemAlignment.Right)
                {
                    End.Add(AttItem);
                }
                else
                {
                    Start.Add(AttItem);
                }
            }
            End.Reverse();
            Start.AddRange(End);
            return Start;
        }

        private List<ToolStripItem> ContentItems()
        {
            return this.DisplayOrderItems();
        }

        /// <summary>true se ci sono elementi che non entrano nella toolbar</summary>
        private bool HasHiddenItems()
        {
            foreach (ToolStripItem AttItem in this.ContentItems())
            {
                if (AttItem.Available && AttItem.Placement != ToolStripItemPlacement.Main)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Applica lo scorrimento corrente nascondendo i primi _ScrollOffset elementi</summary>
        private void ApplyScroll()
        {
            if (this.MainToolbar.IsDisposed)
            {
                return;
            }
            List<ToolStripItem> Content = this.ContentItems();
            if (this._OverflowMode != OverflowModeScroll)
            {
                this._ScrollOffset = 0;
            }
            if (this._ScrollOffset > Content.Count - 1)
            {
                this._ScrollOffset = Math.Max(0, Content.Count - 1);
            }
            this.MainToolbar.SuspendLayout();
            for (int i = 0; i < Content.Count; i++)
            {
                bool Show = i >= this._ScrollOffset;
                if (Content[i].Available != Show)
                {
                    Content[i].Available = Show;
                }
            }
            this.MainToolbar.ResumeLayout(true);
            this.UpdateOverflowButtons();
        }

        private void MainToolbar_LayoutCompleted(object sender, EventArgs e)
        {
            this.UpdateOverflowButtons();
        }

        private static void SetAvailable(ToolStripItem Item, bool Value)
        {
            if (Item.Available != Value)
            {
                Item.Available = Value;
            }
        }

        private void UpdateOverflowButtons()
        {
            if (this._UpdatingOverflowButtons || this._BtnMore == null || this._BtnMore.IsDisposed
                || this._BtnScrollBack.IsDisposed || this._BtnScrollForward.IsDisposed
                || this._BtnMore.Owner != this.MainToolbar)
            {
                return;
            }
            this._UpdatingOverflowButtons = true;
            try
            {
                bool Hidden = this.HasHiddenItems();
                bool Scroll = this._OverflowMode == OverflowModeScroll;
                bool ShowArrows = Scroll && (Hidden || this._ScrollOffset > 0);
                SetAvailable(this._BtnScrollBack, ShowArrows);
                SetAvailable(this._BtnScrollForward, ShowArrows);
                SetAvailable(this._BtnMore, !Scroll && Hidden);
                if (this._BtnScrollBack.Enabled != (this._ScrollOffset > 0))
                {
                    this._BtnScrollBack.Enabled = this._ScrollOffset > 0;
                }
                if (this._BtnScrollForward.Enabled != Hidden)
                {
                    this._BtnScrollForward.Enabled = Hidden;
                }
            }
            finally
            {
                this._UpdatingOverflowButtons = false;
            }
        }

        #region Modalità scorrimento

        private void ScrollBy(int Delta)
        {
            for (int i = 0; i < Math.Abs(Delta); i++)
            {
                if (Delta < 0)
                {
                    if (this._ScrollOffset == 0)
                    {
                        break;
                    }
                    this._ScrollOffset--;
                }
                else
                {
                    if (!this.HasHiddenItems())
                    {
                        break;
                    }
                    this._ScrollOffset++;
                }
                this.ApplyScroll();
            }
        }

        private void BtnScrollBack_Click(object sender, EventArgs e)
        {
            this.ScrollBy(-1);
        }

        private void BtnScrollForward_Click(object sender, EventArgs e)
        {
            this.ScrollBy(1);
        }

        private void Toolbar_MouseWheel(object sender, MouseEventArgs e)
        {
            if (this._OverflowMode != OverflowModeScroll || e.Delta == 0)
            {
                return;
            }
            int Steps = Math.Max(1, Math.Abs(e.Delta) / 120) * 2;
            this.ScrollBy(e.Delta > 0 ? -Steps : Steps);
            HandledMouseEventArgs Handled = e as HandledMouseEventArgs;
            if (Handled != null)
            {
                Handled.Handled = true;
            }
        }

        #endregion

        #region Modalità elenco a comparsa

        private void BtnMore_Click(object sender, EventArgs e)
        {
            if (this._OverflowPopup != null)
            {
                this.CloseOverflowPopup();
                return;
            }
            //il clic sul pulsante » con l'elenco aperto lo chiude già (clic "esterno"):
            //in quel caso non va riaperto subito
            if ((DateTime.Now - this._PopupClosedAt).TotalMilliseconds < 300)
            {
                return;
            }
            this.ShowOverflowPopup();
        }

        private void CloseOverflowPopup()
        {
            ToolStripDropDown Popup = this._OverflowPopup;
            this._OverflowPopup = null;
            if (Popup != null && !Popup.IsDisposed)
            {
                Popup.Close();
            }
        }

        private void ShowOverflowPopup()
        {
            this.CloseOverflowPopup();
            List<ToolStripItem> Hidden = this.ContentItems()
                .Where(it => it.Available && it.Placement != ToolStripItemPlacement.Main && !(it is ToolStripSeparator))
                .ToList();
            if (Hidden.Count == 0)
            {
                return;
            }

            bool Vertical = this.MainToolbar.LayoutStyle == ToolStripLayoutStyle.VerticalStackWithOverflow;
            Rectangle Work = Screen.FromControl(this).WorkingArea;
            int ScrollBarWidth = SystemInformation.VerticalScrollBarWidth;

            //Dimensione della cella: uguale per tutti gli elementi, così l'elenco è in colonne
            int PrefWidth = 0;
            int PrefHeight = 0;
            foreach (ToolStripItem AttItem in Hidden)
            {
                Size Pref = AttItem.GetPreferredSize(Size.Empty);
                PrefWidth = Math.Max(PrefWidth, Pref.Width);
                PrefHeight = Math.Max(PrefHeight, Pref.Height);
            }
            Size Cell;
            int Columns;
            if (Vertical)
            {
                //stessa larghezza della toolbar, eventualmente su più colonne
                Cell = new Size(Math.Max(this.MainToolbar.Width, this.Scaled(24)), Math.Max(PrefHeight, this.Scaled(22)));
                Columns = Math.Min(this._OverflowColumns, Hidden.Count);
                Columns = Math.Max(1, Math.Min(Columns, (Work.Width / 2) / Cell.Width));
            }
            else
            {
                //toolbar orizzontale: tante colonne quante ne entrano nella sua larghezza
                Cell = new Size(Math.Min(Math.Max(PrefWidth, this.Scaled(24)), this.Scaled(250)), Math.Max(PrefHeight, this.Scaled(22)));
                Columns = Math.Max(1, (this.MainToolbar.Width - ScrollBarWidth) / Cell.Width);
                Columns = Math.Min(Columns, Hidden.Count);
            }
            int Rows = (Hidden.Count + Columns - 1) / Columns;
            Size ContentSize = new Size(Columns * Cell.Width, Rows * Cell.Height);
            int MaxHeight = Work.Height - 2;
            bool NeedScroll = ContentSize.Height > MaxHeight;
            Size ViewSize = new Size(ContentSize.Width + (NeedScroll ? ScrollBarWidth : 0), Math.Min(ContentSize.Height, MaxHeight));

            ToolStripDropDown Popup = new ToolStripDropDown();

            //ToolStrip interna con layout a tabella
            ToolStrip Inner = new ToolStrip();
            Inner.Dock = DockStyle.None;
            Inner.AutoSize = false;
            Inner.GripStyle = ToolStripGripStyle.Hidden;
            Inner.CanOverflow = false;
            Inner.LayoutStyle = ToolStripLayoutStyle.Table;
            ((TableLayoutSettings)Inner.LayoutSettings).ColumnCount = Columns;
            Inner.Padding = Padding.Empty;
            Inner.Renderer = this.MainToolbar.Renderer;
            Inner.Font = this.MainToolbar.Font;
            Inner.ForeColor = this.MainToolbar.ForeColor;
            Color Back = this.MainToolbar.BackColor;
            if (Back == Color.Black)
            {
                //effetto Aero: lo sfondo nero è trasparente solo sulla toolbar
                Back = SystemColors.Control;
            }
            Inner.BackColor = Back;
            Inner.ImageScalingSize = this.MainToolbar.ImageScalingSize;
            Inner.AllowDrop = true;
            Inner.ShowItemToolTips = true;
            foreach (ToolStripItem AttItem in Hidden)
            {
                ToolStripItem Clone = this.CloneToolbarItem(AttItem, Popup);
                if (Clone != null)
                {
                    Clone.AutoSize = false;
                    Clone.Size = Cell;
                    Clone.Margin = Padding.Empty;
                    Inner.Items.Add(Clone);
                }
            }
            Inner.Location = Point.Empty;
            Inner.Size = ContentSize;

            Panel View = new Panel();
            View.AutoScroll = NeedScroll;
            View.Size = ViewSize;
            View.BackColor = Back;
            View.Margin = Padding.Empty;
            View.Padding = Padding.Empty;
            View.Controls.Add(Inner);
            Inner.MouseWheel += delegate(object s, MouseEventArgs me)
            {
                if (View.AutoScroll && me.Delta != 0)
                {
                    int NewY = -View.AutoScrollPosition.Y - Math.Sign(me.Delta) * Cell.Height * 3;
                    View.AutoScrollPosition = new Point(0, Math.Max(0, NewY));
                }
                HandledMouseEventArgs Handled = me as HandledMouseEventArgs;
                if (Handled != null)
                {
                    Handled.Handled = true;
                }
            };

            ToolStripControlHost Host = new ToolStripControlHost(View);
            Host.AutoSize = false;
            Host.Size = ViewSize;
            Host.Margin = Padding.Empty;
            Host.Padding = Padding.Empty;

            Popup.AutoSize = false;
            Popup.Padding = new Padding(1);
            Popup.Items.Add(Host);
            Popup.Size = new Size(ViewSize.Width + 2, ViewSize.Height + 2);
            Popup.Closed += delegate(object s, ToolStripDropDownClosedEventArgs ce)
            {
                this.OverflowPopup_Closed(Popup, Inner);
            };

            //Posizione: accanto alla toolbar, allineato al pulsante »
            Rectangle MoreRect = this._BtnMore.Bounds;
            Point MoreScreen = this.MainToolbar.PointToScreen(MoreRect.Location);
            Point Location;
            switch (this.Edge)
            {
                case AppBarEdges.Left:
                    Location = new Point(this.Bounds.Right, MoreScreen.Y + MoreRect.Height - Popup.Height);
                    break;
                case AppBarEdges.Right:
                    Location = new Point(this.Bounds.Left - Popup.Width, MoreScreen.Y + MoreRect.Height - Popup.Height);
                    break;
                case AppBarEdges.Top:
                    Location = new Point(MoreScreen.X + MoreRect.Width - Popup.Width, this.Bounds.Bottom);
                    break;
                case AppBarEdges.Bottom:
                    Location = new Point(MoreScreen.X + MoreRect.Width - Popup.Width, this.Bounds.Top - Popup.Height);
                    break;
                default:
                    Location = Cursor.Position;
                    break;
            }
            //lungo la toolbar l'elenco deve restare dentro l'area di lavoro dello schermo
            if (this.Edge == AppBarEdges.Left || this.Edge == AppBarEdges.Right)
            {
                Location.Y = Math.Max(Work.Top, Math.Min(Location.Y, Work.Bottom - Popup.Height));
            }
            else if (this.Edge == AppBarEdges.Top || this.Edge == AppBarEdges.Bottom)
            {
                Location.X = Math.Max(Work.Left, Math.Min(Location.X, Work.Right - Popup.Width));
            }

            this._OverflowPopup = Popup;
            this._PopupOpen = true;
            this.timerAutoHide.Enabled = false;
            Popup.Show(Location);
        }

        private ToolStripItem CloneToolbarItem(ToolStripItem Source, ToolStripDropDown Popup)
        {
            ToolStripButton SourceBtn = Source as ToolStripButton;
            if (SourceBtn != null)
            {
                ToolStripButton Btn = new ToolStripButton();
                if (SourceBtn.Image != null)
                {
                    Btn.Image = (Image)SourceBtn.Image.Clone();
                }
                Btn.Text = SourceBtn.Text;
                Btn.ToolTipText = SourceBtn.ToolTipText;
                Btn.Tag = SourceBtn.Tag;
                Btn.DisplayStyle = SourceBtn.DisplayStyle;
                Btn.TextImageRelation = SourceBtn.TextImageRelation;
                Btn.ImageAlign = SourceBtn.ImageAlign;
                Btn.TextAlign = SourceBtn.TextAlign;
                Btn.ImageScaling = SourceBtn.ImageScaling;
                Btn.AllowDrop = true;
                Btn.Click += delegate(object s, EventArgs e)
                {
                    Popup.Close();
                    this.ToolStripButton_Click(Btn, e);
                };
                Btn.MouseDown += new MouseEventHandler(this.ToolStripButton_MouseDown);
                Btn.DragEnter += new DragEventHandler(this.ToolStripButton_DragEnter);
                Btn.DragDrop += new DragEventHandler(this.ToolStripButton_DragDrop);
                return Btn;
            }
            ToolStripLabel SourceLbl = Source as ToolStripLabel;
            if (SourceLbl != null)
            {
                ToolStripLabel Lbl = new ToolStripLabel(SourceLbl.Text);
                Lbl.ToolTipText = SourceLbl.ToolTipText;
                Lbl.Tag = SourceLbl.Tag;
                Lbl.TextAlign = ContentAlignment.MiddleLeft;
                Lbl.Font = new Font(this.MainToolbar.Font, FontStyle.Bold);
                if (!this._CustomTextColor.IsEmpty)
                {
                    Lbl.ForeColor = this._CustomTextColor;
                }
                Lbl.Click += new EventHandler(this.ToolStripLabel_DoubleClick);
                return Lbl;
            }
            return null;
        }

        private void OverflowPopup_Closed(ToolStripDropDown Popup, ToolStrip Inner)
        {
            if (this._OverflowPopup == Popup)
            {
                this._OverflowPopup = null;
            }
            this._PopupOpen = false;
            this._PopupClosedAt = DateTime.Now;
            //rilascio risorse a chiusura completata
            this.BeginInvoke(new MethodInvoker(delegate
            {
                foreach (ToolStripItem AttItem in Inner.Items)
                {
                    if (AttItem.Image != null)
                    {
                        AttItem.Image.Dispose();
                    }
                    if (AttItem is ToolStripLabel && AttItem.Font != null)
                    {
                        AttItem.Font.Dispose();
                    }
                }
                Popup.Dispose();
            }));
            //riprendo l'auto-hide se il mouse non è sulla toolbar
            if (this._AutoHide && !this.ToolbarIsHide && !this.Bounds.Contains(Cursor.Position))
            {
                this.timerAutoHide.Interval = this._AutoHideInterval;
                this.timerAutoHide.Enabled = true;
            }
        }

        #endregion
    }
}
