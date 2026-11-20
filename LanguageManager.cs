using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Windows.Forms;

using System.ComponentModel;
//using System.Collections;
//using System.Reflection;
//using System.Diagnostics;
//using System.ComponentModel.Design;
//using System.Drawing;
//using System.Resources;

namespace SevenDeskBar
{
    class LanguageManager
    {
        public static void ChangeLanguage(object sender)
        {
            SevenDeskBar.Properties.Settings MySettings = new SevenDeskBar.Properties.Settings();
            MySettings.Reload();
            switch (MySettings.LanguageSelected)
            {
                case (int)ApplLanguage.Italian:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("it-IT");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("it-IT");
                    break;
                case (int)ApplLanguage.English:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    break;
                case (int)ApplLanguage.Francais:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
                    break;
                case (int)ApplLanguage.Espanol:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");
                    break;
                case (int)ApplLanguage.Deutsch:
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
                    break;
            }
            ChangeObjLang(sender);
            
        }

        public static void ChangeLanguage(object sender, string CurCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(CurCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(CurCulture);
            ChangeObjLang(sender);
        }

        private static void ChangeObjLang(object sender)
        {
            //La toolbar non ha testi da tradurre (il menu contestuale viene tradotto
            //all'apertura): riapplicare le risorse ne ripristinerebbe dimensioni e
            //posizione originali, causando inutili riposizionamenti della barra.
            if (sender is DesktopToolbar)
            {
                return;
            }
            if (sender is System.Windows.Forms.Form)
            {
                System.Windows.Forms.Form CurForm = (System.Windows.Forms.Form)sender;
                CurForm.SuspendLayout();
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(CurForm.GetType());
                resources.ApplyResources(CurForm, "$this");
                foreach (Control childControl in CurForm.Controls)
                {
                    resources.ApplyResources(childControl, childControl.Name );
                    if (childControl is ComboBox)
                    {
                        //Apply resource to items
                        ComboBox AttCombo = (ComboBox)childControl;
                        int NumItems = AttCombo.Items.Count ;
                        int SelItem = AttCombo.SelectedIndex;
                        AttCombo.Items.Clear();
                        for (int i = 0; i < NumItems; i++)
                        {
                            object[] MyString = new object[] { resources.GetString(AttCombo.Name.Trim() + ".Items" + (i==0?"":i.ToString().Trim()))};
                            AttCombo.Items.AddRange(MyString);
                        }
                        AttCombo.SelectedIndex = SelItem;
                        AttCombo = null;
                    }
                }
                //foreach (Component childComponent in CurForm.Contains)
                //{
                    //resources.ApplyResources(childComponent, childComponent);
                //}
                CurForm.ResumeLayout();
            }
        }

        public static void ChangeContextMenuLang(object sender)
        {
            ContextMenuStrip AttMenu = (ContextMenuStrip)sender;
            System.Windows.Forms.Form CurForm = (System.Windows.Forms.Form)AttMenu.SourceControl;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(CurForm.GetType());            
            for (int i = 0; i < AttMenu.Items.Count; i++)
            {
                resources.ApplyResources(AttMenu.Items[i], AttMenu.Items[i].Name);
            }
            AttMenu = null;

        }

    }
}
