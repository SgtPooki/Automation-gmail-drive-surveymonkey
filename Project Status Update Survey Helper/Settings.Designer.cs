namespace Project_Status_Update_Survey_Helper
{
    partial class frmSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtSignInEmail = new System.Windows.Forms.TextBox();
            this.txtFromEmail = new System.Windows.Forms.TextBox();
            this.txtSpreadsheetName = new System.Windows.Forms.TextBox();
            this.txtEmailSubject = new System.Windows.Forms.TextBox();
            this.lblSignInEmail = new System.Windows.Forms.Label();
            this.lblFromEmail = new System.Windows.Forms.Label();
            this.lblSpreadsheetName = new System.Windows.Forms.Label();
            this.lblEmailSubject = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtSignInEmail
            // 
            this.txtSignInEmail.Location = new System.Drawing.Point(16, 34);
            this.txtSignInEmail.Name = "txtSignInEmail";
            this.txtSignInEmail.Size = new System.Drawing.Size(232, 20);
            this.txtSignInEmail.TabIndex = 0;
            // 
            // txtFromEmail
            // 
            this.txtFromEmail.Location = new System.Drawing.Point(16, 85);
            this.txtFromEmail.Name = "txtFromEmail";
            this.txtFromEmail.Size = new System.Drawing.Size(232, 20);
            this.txtFromEmail.TabIndex = 1;
            // 
            // txtSpreadsheetName
            // 
            this.txtSpreadsheetName.Location = new System.Drawing.Point(15, 140);
            this.txtSpreadsheetName.Name = "txtSpreadsheetName";
            this.txtSpreadsheetName.Size = new System.Drawing.Size(233, 20);
            this.txtSpreadsheetName.TabIndex = 2;
            // 
            // txtEmailSubject
            // 
            this.txtEmailSubject.Location = new System.Drawing.Point(16, 185);
            this.txtEmailSubject.Name = "txtEmailSubject";
            this.txtEmailSubject.Size = new System.Drawing.Size(232, 20);
            this.txtEmailSubject.TabIndex = 3;
            // 
            // lblSignInEmail
            // 
            this.lblSignInEmail.AutoSize = true;
            this.lblSignInEmail.Location = new System.Drawing.Point(13, 18);
            this.lblSignInEmail.Name = "lblSignInEmail";
            this.lblSignInEmail.Size = new System.Drawing.Size(72, 13);
            this.lblSignInEmail.TabIndex = 4;
            this.lblSignInEmail.Text = "Sign in email: ";
            // 
            // lblFromEmail
            // 
            this.lblFromEmail.AutoSize = true;
            this.lblFromEmail.Location = new System.Drawing.Point(13, 69);
            this.lblFromEmail.Name = "lblFromEmail";
            this.lblFromEmail.Size = new System.Drawing.Size(63, 13);
            this.lblFromEmail.TabIndex = 5;
            this.lblFromEmail.Text = "From email: ";
            // 
            // lblSpreadsheetName
            // 
            this.lblSpreadsheetName.AutoSize = true;
            this.lblSpreadsheetName.Location = new System.Drawing.Point(12, 124);
            this.lblSpreadsheetName.Name = "lblSpreadsheetName";
            this.lblSpreadsheetName.Size = new System.Drawing.Size(99, 13);
            this.lblSpreadsheetName.TabIndex = 6;
            this.lblSpreadsheetName.Text = "Spreadsheet name:";
            // 
            // lblEmailSubject
            // 
            this.lblEmailSubject.AutoSize = true;
            this.lblEmailSubject.Location = new System.Drawing.Point(13, 169);
            this.lblEmailSubject.Name = "lblEmailSubject";
            this.lblEmailSubject.Size = new System.Drawing.Size(168, 13);
            this.lblEmailSubject.TabIndex = 7;
            this.lblEmailSubject.Text = "Text in subject of email to look for:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(88, 216);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(173, 216);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 251);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblEmailSubject);
            this.Controls.Add(this.lblSpreadsheetName);
            this.Controls.Add(this.lblFromEmail);
            this.Controls.Add(this.lblSignInEmail);
            this.Controls.Add(this.txtEmailSubject);
            this.Controls.Add(this.txtSpreadsheetName);
            this.Controls.Add(this.txtFromEmail);
            this.Controls.Add(this.txtSignInEmail);
            this.Name = "frmSettings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSignInEmail;
        private System.Windows.Forms.TextBox txtFromEmail;
        private System.Windows.Forms.TextBox txtSpreadsheetName;
        private System.Windows.Forms.TextBox txtEmailSubject;
        private System.Windows.Forms.Label lblSignInEmail;
        private System.Windows.Forms.Label lblFromEmail;
        private System.Windows.Forms.Label lblSpreadsheetName;
        private System.Windows.Forms.Label lblEmailSubject;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}