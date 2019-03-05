using System;
using System.Drawing;
using System.Windows.Forms;
using BrowserSelect.DomainModels;

namespace BrowserSelect.CustomControls {
    public partial class BrowserUC : DevExpress.XtraEditors.XtraUserControl
    {
        public Browser browser;
        public BrowserUC(Browser b,int index) {
            InitializeComponent();

            this.browser = b;

            name.Text = b.BrowserName;
            shortcuts.Text = "( " + Convert.ToString(index+1) + "," + String.Join(",", b.shortcuts) + " )";
            shortcuts.ForeColor = Color.FromKnownColor(KnownColor.GrayText);
            icon.Image = b.BrowserIcon.ToBitmap();
            icon.SizeMode = PictureBoxSizeMode.Zoom;
        }
        public new event EventHandler Click {
            add {
                base.Click += value;
                foreach (Control control in Controls) {
                    control.Click += value;
                }
            }
            remove {
                base.Click -= value;
                foreach (Control control in Controls) {
                    control.Click -= value;
                }
            }
        }

        public bool Always { get; set; } = false;

        private void button1_Click(object sender, EventArgs e)
        {
            Always = true;
        }
    }
}
