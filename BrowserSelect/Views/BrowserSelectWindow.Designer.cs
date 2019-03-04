namespace BrowserSelect.Views {
    partial class BrowserSelectWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowserSelectWindow));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_help = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // btn_help
            // 
            this.btn_help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_help.BackgroundImage = global::BrowserSelect.Properties.Resources.Button_help_icon;
            this.btn_help.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_help.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btn_help.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_help.ImageOptions.Location = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.btn_help.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btn_help.ImageOptions.SvgImage")));
            this.btn_help.ImageOptions.SvgImageSize = new System.Drawing.Size(30, 30);
            this.btn_help.Location = new System.Drawing.Point(97, 155);
            this.btn_help.Name = "btn_help";
            this.btn_help.Size = new System.Drawing.Size(25, 25);
            this.btn_help.TabIndex = 0;
            this.btn_help.Click += new System.EventHandler(this.btn_help_Click);
            // 
            // BrowserSelectWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(124, 181);
            this.Controls.Add(this.btn_help);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "BrowserSelectWindow";
            this.Text = "Browser Select";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private DevExpress.XtraEditors.SimpleButton btn_help;
    }
}

