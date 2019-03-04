using System;

namespace BrowserSelect.Views
{
    public partial class MainHelpWindow : DevExpress.XtraEditors.XtraForm
    {
        public MainHelpWindow()
        {
            InitializeComponent();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
