using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenDeskBar
{
    internal class VistaApi
    {
        //[System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        //internal static extern void DwmExtendFrameIntoClientArea(System.IntPtr hWnd, ref Margins pMargins);

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        internal static extern void DwmIsCompositionEnabled(ref bool isEnabled);

        [System.Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);


        public struct DWM_BLURBEHIND
        {

            public int dwFlags;

            public bool fEnable;

            public IntPtr hRgnBlur;

            public bool fTransitionOnMaximized;
        }
        public static int DWM_BB_ENABLE = 1;
        public static int DWM_BB_BLURREGION = 2;
        public static int DWM_BB_TRANSITIONONMAXIMIZED = 0;

        /*internal struct Margins
        {
            public int Left, Right, Top, Bottom;
        }*/

        public static bool IsGlassEnabled()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                return false;
            }

            //Check if DWM is enabled
            bool isGlassSupported = false;
            VistaApi.DwmIsCompositionEnabled(ref isGlassSupported);
            return isGlassSupported;
        }
    }
}
