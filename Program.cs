using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace SevenDeskBar
{
    public enum ApplLanguage
    {
        Italian = 0,
        English = 1,
        Francais = 2,
        Espanol = 3,
        Deutsch = 4
    }

    

    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        /// 
        public static bool bRebootNeeded = false;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Program.bRebootNeeded = true;
            while (Program.bRebootNeeded)
            {
                Program.bRebootNeeded = false;
                Application.Run(new MainForm());
            }
        }

        

        
    }
}
