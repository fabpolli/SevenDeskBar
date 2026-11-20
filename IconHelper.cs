using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace SevenDeskBar
{
    /// <summary>
    /// Icone di file, cartelle e collegamenti alla dimensione esatta richiesta.
    ///  - supporto DPI: a 150% un'icona "piccola" è di 24 pixel, una "grande" di 48;
    ///  - ticket #18: alcune icone (collegamenti a cartelle, collegamenti non validi...)
    ///    venivano restituite solo grandi e disegnate tagliate: ora ogni immagine viene
    ///    sempre ridimensionata alla stessa misura delle altre.
    /// </summary>
    internal static class IconHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFOW
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFOW psfi, uint cbFileInfo, uint uFlags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        private static extern int SHGetImageList(int iImageList, ref Guid riid, out IntPtr ppv);

        [DllImport("comctl32.dll")]
        private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, uint flags);

        [DllImport("comctl32.dll")]
        private static extern bool ImageList_GetIconSize(IntPtr himl, out int cx, out int cy);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_SYSICONINDEX = 0x4000;
        private const uint ILD_TRANSPARENT = 0x1;
        private const int SHIL_LARGE = 0x0;
        private const int SHIL_SMALL = 0x1;
        private const int SHIL_EXTRALARGE = 0x2;
        private const int SHIL_JUMBO = 0x4;
        private static Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

        /// <summary>
        /// Icona del file/cartella/collegamento alla dimensione indicata (in pixel),
        /// oppure null se non è possibile ottenerla.
        /// </summary>
        public static Bitmap GetBitmap(string Path, int Size)
        {
            try
            {
                SHFILEINFOW info = new SHFILEINFOW();
                IntPtr res = SHGetFileInfo(Path, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_SYSICONINDEX);
                if (res == IntPtr.Zero)
                {
                    return null;
                }

                //scelgo la lista di immagini di sistema più piccola che sia almeno grande quanto serve
                //(la lista "jumbo" solo se necessario: le icone senza versione 256px vi
                //compaiono piccole al centro di un riquadro vuoto)
                int[] Lists = new int[] { SHIL_SMALL, SHIL_LARGE, SHIL_EXTRALARGE, SHIL_JUMBO };
                Bitmap Best = null;
                foreach (int ListId in Lists)
                {
                    Bitmap Candidate = GetFromImageList(ListId, info.iIcon);
                    if (Candidate == null)
                    {
                        continue;
                    }
                    if (Best != null)
                    {
                        Best.Dispose();
                    }
                    Best = Candidate;
                    if (Candidate.Width >= Size)
                    {
                        break;
                    }
                }
                if (Best == null)
                {
                    return null;
                }
                Bitmap Result = Resize(Best, Size);
                if (Result != Best)
                {
                    Best.Dispose();
                }
                return Result;
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap GetFromImageList(int ListId, int Index)
        {
            IntPtr himl;
            Guid iid = IID_IImageList;
            if (SHGetImageList(ListId, ref iid, out himl) != 0 || himl == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                IntPtr hIcon = ImageList_GetIcon(himl, Index, ILD_TRANSPARENT);
                if (hIcon == IntPtr.Zero)
                {
                    return null;
                }
                try
                {
                    using (Icon Ico = Icon.FromHandle(hIcon))
                    {
                        return Ico.ToBitmap();
                    }
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
            finally
            {
                Marshal.Release(himl);
            }
        }

        /// <summary>Converte un'icona in bitmap della dimensione indicata</summary>
        public static Bitmap FromIcon(Icon Source, int Size)
        {
            if (Source == null)
            {
                return null;
            }
            using (Bitmap Raw = Source.ToBitmap())
            {
                Bitmap Result = Resize(Raw, Size);
                return Result == Raw ? new Bitmap(Raw) : Result;
            }
        }

        /// <summary>
        /// Ridimensiona (in alta qualità) un'immagine a Size x Size pixel.
        /// Restituisce l'immagine originale se ha già la dimensione giusta.
        /// </summary>
        public static Bitmap Resize(Bitmap Source, int Size)
        {
            if (Source.Width == Size && Source.Height == Size)
            {
                return Source;
            }
            Bitmap Result = new Bitmap(Size, Size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(Result))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                //mantengo le proporzioni
                float Scale = Math.Min((float)Size / Source.Width, (float)Size / Source.Height);
                float w = Source.Width * Scale;
                float h = Source.Height * Scale;
                g.DrawImage(Source, (Size - w) / 2f, (Size - h) / 2f, w, h);
            }
            return Result;
        }
    }
}
