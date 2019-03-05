using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrowserSelect.DomainModels;
using BrowserSelect.Properties;
using BrowserSelect.Services;
using DevExpress.XtraEditors;
using Microsoft.Win32;
using ItemCheckEventArgs = DevExpress.XtraEditors.Controls.ItemCheckEventArgs;

namespace BrowserSelect.Views
{
    public partial class SettingsWindowDx : DevExpress.XtraEditors.XtraForm
    {
        private List<FilterAutoMatchRule> AutoMatchRules = new List<FilterAutoMatchRule>();
        private HelpRulesWindow HelpWindow;
        public SettingsWindowDx()
        {
            InitializeComponent();
            HookUpEvents();
        }
        private void HookUpEvents()
        {
            Load += (sender, args) => SetupWindow();
            simpleButtonSetDefaultBrowser.Click += SimpleButtonSetDefaultBrowserOnClick;
            checkedListBoxControlBrowserFilters.ItemCheck += CheckedListBoxControlBrowserFiltersOnItemCheck;
            labelControlFilterInstructions.HyperlinkClick += (sender, args) =>
                Process.Start("https://github.com/zumoshi/BrowserSelect/blob/master/help/filters.md");
            simpleButtonApply.Click += SimpleButtonApplyOnClick;
            simpleButtonHelp.Click += SimpleButtonHelpOnClick;
            gridViewFilters.ShownEditor += (sender, args) => EditingBegunForGrid();
            gridViewFilters.RowDeleting += (sender, args) => EditingBegunForGrid();
            gridViewFilters.DataSourceChanged += (sender, args) => EditingBegunForGrid();
            simpleButtonCheckForUpdates.Click += SimpleButtonCheckForUpdatesOnClick;
            checkEditEnableUpdates.CheckedChanged += CheckEditEnableUpdatesOnCheckedChanged;
            Closing += OnClosing;
            simpleButtonClose.Click += (sender, args) =>
            {
                simpleButtonApply.Enabled = false;
                Close();
            };
        }
        private void SimpleButtonCheckForUpdatesOnClick(object sender, EventArgs e)
        {
            var btn = ((Button)sender);
            var uc = new UpdateChecker();
            // color the button to indicate request, disable it to prevent multiple instances
            btn.BackColor = Color.Blue;
            btn.Enabled = false;
            // run inside a Task to prevent freezing the UI
            Task.Factory.StartNew(() => uc.check()).ContinueWith(x =>
            {
                try
                {
                    if (uc.Checked)
                    {
                        if (uc.Updated)
                            MessageBox.Show(String.Format(
                                "New Update Available!\nCurrent Version: {1}\nLast Version: {0}" +
                                "\nto Update download and install the new version from project's github.",
                                uc.LVer, uc.CVer));
                        else
                            MessageBox.Show("You are running the lastest version.");
                    }
                    else
                        MessageBox.Show("Unable to check for updates.\nPlease make sure you are connected to internet.");
                    btn.UseVisualStyleBackColor = true;
                    btn.Enabled = true;
                }
                catch (Exception) { }
                return x;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        private void CheckEditEnableUpdatesOnCheckedChanged(object sender, EventArgs e)
        {


            Settings.Default.check_update = (((CheckBox)sender).Checked) ? "0" : "nope";
            Settings.Default.Save();
        }
        private void EditingBegunForGrid()
        {
            //set the unsaved changes flag to true
            simpleButtonApply.Enabled = true;
            simpleButtonClose.Text = "Cancel";
        }
        private void SimpleButtonHelpOnClick(object sender, EventArgs e)
        {
            if (HelpWindow != null)
            {
                HelpWindow.Focus();
            }
            else
            {
                HelpWindow = new HelpRulesWindow();
                HelpWindow.FormClosed += (theSender, arguments) => HelpWindow = null;
                HelpWindow.Show();
            }
        }
        private void SimpleButtonApplyOnClick(object sender, EventArgs e)
        {
            //save rules

            //clear rules (instead of checking for changes we just overwrite the whole ruleset)
            Settings.Default.AutoBrowser.Clear();
            foreach (var rule in AutoMatchRules)
            {
                //check if rule has both pattern and browser defined
                if (rule.valid())
                    //add it to rule list
                    Settings.Default.AutoBrowser.Add(rule.ToString());
                else
                {
                    //ignore rule if both pattern and browser is empty otherwise inform user of missing part
                    var err = rule.error();
                    if (err.Length > 0)
                    {
                        MessageBox.Show("Invalid Rule: " + err);
                    }
                }

            }
            //save rules
            Settings.Default.Save();
            //Enabled property of apply button is used as a flag for unsaved changes
            simpleButtonApply.Enabled = false;
            simpleButtonClose.Text = "Close";
        }
        private void SimpleButtonSetDefaultBrowserOnClick(object sender, EventArgs e)
        {
            //set browser select as default in registry

            //http
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                key.SetValue("ProgId", "bselectURL");
            }
            //https
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice"))
            {
                key.SetValue("ProgId", "bselectURL");
            }

            simpleButtonSetDefaultBrowser.Enabled = false;
        }
        private void CheckedListBoxControlBrowserFiltersOnItemCheck(object sender, ItemCheckEventArgs e)
        {
            //Save changes to the BrowserFilter List
            if (!(checkedListBoxControlBrowserFilters.Items[e.Index].Value is Browser selectedBrowser)) return;
            if (e.State == CheckState.Checked)
            {
                Settings.Default.HideBrowsers.Remove(selectedBrowser.ExecutablePath);
            }
            else
            {
                Settings.Default.HideBrowsers.Add(selectedBrowser.ExecutablePath);
            }
            Settings.Default.Save();
        }
        private void SetupWindow()
        {
            //check if browser select is the default browser or not
            //to disable/enable "set Browser select as default" button
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                var default_browser = key?.GetValue("ProgId");

                //disable the set default if already default
                if (default_browser != null && (string) default_browser == "bselectURL")
                    simpleButtonSetDefaultBrowser.Enabled = false;
            }

            //populate list of browsers for Rule List ComboBox
            var browsers = BrowserFinder.find();
            var browserNameList = new List<string>();
            browserNameList.Add("display BrowserSelect");
            foreach (Browser b in browsers)
            {
                checkedListBoxControlBrowserFilters.Items.Add(b, !Settings.Default.HideBrowsers.Contains(b.ExecutablePath));
                browserNameList.Add(b.BrowserName);
            }

            // add browser select to the list
            repositoryItemLookUpEditBrowser.DataSource = browserNameList;
            //populate Rules in the gridview
            foreach (var rule in Settings.Default.AutoBrowser)
                AutoMatchRules.Add(rule);
            var gridBindingSource = new BindingSource {DataSource = AutoMatchRules};
            gridControlFilters.DataSource = gridBindingSource;
            new DxGridMultiSelectEditor(gridViewFilters);
            checkEditEnableUpdates.Checked = Settings.Default.check_update != "nope";
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!simpleButtonApply.Enabled) return;
            var window = XtraMessageBox.Show("You have unsaved changes, are you sure you want to close without saving ?",
                "Unsaved Changes", MessageBoxButtons.YesNo);
            e.Cancel = window == DialogResult.No;
        }
    }
}