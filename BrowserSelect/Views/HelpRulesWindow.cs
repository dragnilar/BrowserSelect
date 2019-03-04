using System;

namespace BrowserSelect.Views
{
    public partial class HelpRulesWindow : DevExpress.XtraEditors.XtraForm
    {
        public HelpRulesWindow()
        {
            InitializeComponent();
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
