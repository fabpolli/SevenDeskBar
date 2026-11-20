using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SevenDeskBar
{
    /// <summary>
    /// Importazione delle impostazioni delle versioni precedenti (1.0.x, .NET Framework 2.0/3.5).
    ///
    /// Il file user.config si trova in
    ///   %LOCALAPPDATA%\SharkSoft\SevenDeskBar.exe_Url_[hash]\[versione]\user.config
    /// dove [hash] dipende dal percorso dell'eseguibile. .NET 2.0 calcolava l'hash su
    /// "FILE:///C:/PERCORSO/..." mentre .NET 4 lo calcola su "FILE:///C:\PERCORSO\...":
    /// con il passaggio a .NET 4.8 la cartella cambia e Settings.Upgrade() non trova più
    /// le impostazioni precedenti, quindi vengono cercate e importate qui.
    /// </summary>
    internal static class LegacySettings
    {
        private const string Base32Chars = "abcdefghijklmnopqrstuvwxyz012345";

        // stessa codifica usata da .NET Framework per i nomi di cartella (IsolatedStorage)
        private static string ToBase32(byte[] buff)
        {
            StringBuilder sb = new StringBuilder();
            int l = buff.Length, i = 0;
            do
            {
                byte b0 = i < l ? buff[i++] : (byte)0, b1 = i < l ? buff[i++] : (byte)0, b2 = i < l ? buff[i++] : (byte)0,
                     b3 = i < l ? buff[i++] : (byte)0, b4 = i < l ? buff[i++] : (byte)0;
                sb.Append(Base32Chars[b0 & 0x1F]); sb.Append(Base32Chars[b1 & 0x1F]); sb.Append(Base32Chars[b2 & 0x1F]);
                sb.Append(Base32Chars[b3 & 0x1F]); sb.Append(Base32Chars[b4 & 0x1F]);
                sb.Append(Base32Chars[((b0 & 0xE0) >> 5) | ((b3 & 0x60) >> 2)]);
                sb.Append(Base32Chars[((b1 & 0xE0) >> 5) | ((b4 & 0x60) >> 2)]);
                b2 >>= 5;
                if ((b3 & 0x80) != 0) b2 |= 0x08;
                if ((b4 & 0x80) != 0) b2 |= 0x10;
                sb.Append(Base32Chars[b2]);
            } while (i < l);
            return sb.ToString();
        }

        private static string HashOf(string Evidence)
        {
            using (MemoryStream ms = new MemoryStream())
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                new BinaryFormatter().Serialize(ms, Evidence);
                ms.Position = 0;
                return ToBase32(sha.ComputeHash(ms));
            }
        }

        /// <summary>Nome della cartella delle impostazioni usato da .NET 2.0 per l'eseguibile</summary>
        public static string Net2FolderName(string ExePath)
        {
            string Url = new Uri(ExePath).AbsoluteUri.ToUpperInvariant();
            return Path.GetFileName(ExePath) + "_Url_" + HashOf(Url);
        }

        /// <summary>
        /// user.config più recente della versione .NET 2.0 per l'eseguibile indicato, o null
        /// </summary>
        public static string FindNet2UserConfig(string ExePath, string CompanyName)
        {
            string Folder = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CompanyName), Net2FolderName(ExePath));
            if (!Directory.Exists(Folder))
            {
                return null;
            }
            string Best = null;
            Version BestVersion = null;
            foreach (string VersionDir in Directory.GetDirectories(Folder))
            {
                Version v;
                string File1 = Path.Combine(VersionDir, "user.config");
                if (Version.TryParse(Path.GetFileName(VersionDir), out v) && File.Exists(File1)
                    && (BestVersion == null || v > BestVersion))
                {
                    BestVersion = v;
                    Best = File1;
                }
            }
            return Best;
        }

        /// <summary>Valori (nome impostazione -> testo) contenuti in un user.config</summary>
        public static Dictionary<string, string> ReadValues(string UserConfigFile)
        {
            Dictionary<string, string> Result = new Dictionary<string, string>();
            XmlDocument Doc = new XmlDocument();
            Doc.Load(UserConfigFile);
            XmlNodeList Nodes = Doc.SelectNodes("/configuration/userSettings/SevenDeskBar.Properties.Settings/setting");
            if (Nodes == null)
            {
                return Result;
            }
            foreach (XmlNode Node in Nodes)
            {
                XmlAttribute Name = Node.Attributes["name"];
                XmlNode Value = Node.SelectSingleNode("value");
                if (Name != null && Value != null)
                {
                    Result[Name.Value] = Value.InnerText;
                }
            }
            return Result;
        }

        /// <summary>
        /// Importa nelle impostazioni correnti quelle della versione precedente, se esistono.
        /// Restituisce true se è stato importato qualcosa (le impostazioni vengono salvate).
        /// </summary>
        public static bool ImportFromPreviousVersion(SevenDeskBar.Properties.Settings MySettings)
        {
            try
            {
                string OldFile = FindNet2UserConfig(Application.ExecutablePath, Application.CompanyName);
                if (OldFile == null)
                {
                    return false;
                }
                Dictionary<string, string> Values = ReadValues(OldFile);
                bool Imported = false;
                foreach (SettingsProperty Prop in MySettings.Properties)
                {
                    string Text;
                    if (!Values.TryGetValue(Prop.Name, out Text))
                    {
                        continue;
                    }
                    try
                    {
                        MySettings[Prop.Name] = Convert.ChangeType(Text, Prop.PropertyType, CultureInfo.InvariantCulture);
                        Imported = true;
                    }
                    catch
                    {
                        //valore non convertibile: resta il predefinito
                    }
                }
                if (Imported)
                {
                    MySettings.Save();
                }
                return Imported;
            }
            catch
            {
                return false;
            }
        }
    }
}
