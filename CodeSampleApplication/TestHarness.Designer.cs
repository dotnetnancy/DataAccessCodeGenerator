namespace CodeSampleApplication
{
    partial class frmTestHarness
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
            this.btnTestInsertCompositeCustomer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTestInsertCompositeCustomer
            // 
            this.btnTestInsertCompositeCustomer.Location = new System.Drawing.Point(39, 38);
            this.btnTestInsertCompositeCustomer.Name = "btnTestInsertCompositeCustomer";
            this.btnTestInsertCompositeCustomer.Size = new System.Drawing.Size(279, 92);
            this.btnTestInsertCompositeCustomer.TabIndex = 0;
            this.btnTestInsertCompositeCustomer.Text = "Test Harness:  Insert Composite Customer";
            this.btnTestInsertCompositeCustomer.UseVisualStyleBackColor = true;
            this.btnTestInsertCompositeCustomer.Click += new System.EventHandler(this.InsertCompositeCustomer_Click);
            // 
            // frmTestHarness
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 192);
            this.Controls.Add(this.btnTestInsertCompositeCustomer);
            this.Name = "frmTestHarness";
            this.Text = "Test Harness";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTestInsertCompositeCustomer;

    }
}

