using System.ComponentModel;

namespace ERD2DB.Windows
{
    partial class Main32W
    {
        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.SuspendLayout();
            // 
            // Main32W
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "Main32W";
            this.Text = "제목 없음 - ERD2DB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}