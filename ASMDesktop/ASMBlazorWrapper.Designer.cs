using System.Drawing;

namespace ASM.GUI.Desktop
{
    partial class ASMBlazorWrapper
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1800, 1080);
            this.Text = "ASM";
            this.Icon = new Icon("Resources/icon.ico");
        }

        #endregion
    }
}

