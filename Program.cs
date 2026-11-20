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

        /// <summary>
        /// Font con cui sono state disegnate le finestre (AutoScaleDimensions 6x13).
        /// Con il supporto DPI per monitor il font predefinito di Windows Forms viene
        /// restituito già "ridotto" per 96 DPI (es. 6,6pt a 125%): la scalatura automatica
        /// non scatterebbe e le finestre resterebbero piccole con i testi tagliati.
        /// Impostandolo esplicitamente prima di InitializeComponent la scalatura funziona.
        /// </summary>
        public static System.Drawing.Font DialogFont()
        {
            return new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
        }

        [STAThread]
        static void Main()
        {
            //Se la barra viene avviata da un terminale di VS Code (o da un altro programma
            //basato su Electron) eredita ELECTRON_RUN_AS_NODE=1 e la passa ai programmi che
            //lancia: Code.exe e le altre applicazioni Electron partirebbero come semplice
            //Node.js e si chiuderebbero subito senza aprire alcuna finestra.
            Environment.SetEnvironmentVariable("ELECTRON_RUN_AS_NODE", null);

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
