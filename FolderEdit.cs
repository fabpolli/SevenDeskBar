using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SevenDeskBar
{
    public partial class FolderEdit : Form
    {
        string AttFolderPath;
        public FolderEdit(string FolderPath)
        {
            InitializeComponent();
            AttFolderPath = FolderPath;
        }

        private void FolderEdit_Load(object sender, EventArgs e )
        {
            this.TextFolder.Text = this.AttFolderPath;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(this.TextFolder.Text))
            {
                this.AttFolderPath = this.TextFolder.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(ResourceMessages.ErrorInvalidPath , ResourceMessages.GenericErrorTitle , MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        public string GetFolderPath()
        {
            return this.AttFolderPath;
        }
    }
}
