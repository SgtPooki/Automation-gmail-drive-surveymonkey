using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Project_Status_Update_Survey_Helper.Properties;

namespace Project_Status_Update_Survey_Helper
{
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
            this.txtEmailSubject.Text = Settings.Default.emailSubject;
            this.txtFromEmail.Text = Settings.Default.fromEmail;
            this.txtSignInEmail.Text = Settings.Default.signInEmail;
            this.txtSpreadsheetName.Text = Settings.Default.spreadsheetName;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings.Default.emailSubject = this.txtEmailSubject.Text;
            Settings.Default.fromEmail = this.txtFromEmail.Text;
            Settings.Default.signInEmail = this.txtSignInEmail.Text;
            Settings.Default.spreadsheetName = this.txtSpreadsheetName.Text;
            Settings.Default.Save();
            this.Close();
        }
    }
}
