﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrowserSelect.DomainModels;
using BrowserSelect.Properties;
using BrowserSelect.Services;
using Microsoft.Win32;

namespace BrowserSelect.Views
{
    public partial class SettingsWindow : DevExpress.XtraEditors.XtraForm
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private List<FilterAutoMatchRule> rules = new List<FilterAutoMatchRule>();
        private void frm_settings_Load(object sender, EventArgs e)
        {
            //check if browser select is the default browser or not
            //to disable/enable "set Browser select as default" button
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                var default_browser = key?.GetValue("ProgId");

                //disable the set default if already default
                if (default_browser != null && (string)default_browser == "bselectURL")
                    btn_setdefault.Enabled = false;
            }

            //populate list of browsers for Rule List ComboBox
            var browsers = BrowserFinder.find();
            var c = ((DataGridViewComboBoxColumn)gv_filters.Columns["browser"]);

            foreach (Browser b in browsers)
            {
                browser_filter.Items.Add(b, !Settings.Default.HideBrowsers.Contains(b.exec));
                c.Items.Add(b.ToString());
            }
            // add browser select to the list
            c.Items.Add("display BrowserSelect");

            //populate Rules in the gridview
            foreach (var rule in Settings.Default.AutoBrowser)
                rules.Add(rule);
            var bs = new BindingSource();
            bs.DataSource = rules;
            gv_filters.DataSource = bs;

            chk_check_update.Checked = Settings.Default.check_update != "nope";
        }

        private void btn_setdefault_Click(object sender, EventArgs e)
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

            btn_setdefault.Enabled = false;
        }

        private void browser_filter_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //Save changes to the BrowserFilter List
            if (e.NewValue == CheckState.Checked)
            {
                Settings.Default.HideBrowsers.Remove(((Browser)browser_filter.Items[e.Index]).exec);
            }
            else
            {
                Settings.Default.HideBrowsers.Add(((Browser)browser_filter.Items[e.Index]).exec);
            }
            Settings.Default.Save();
        }

        private void frm_settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            // alert user of unsaved changes
            if (btn_apply.Enabled)
            {
                var window = MessageBox.Show("You have unsaved changes, are you sure you want to close without saving ?",
                    "Unsaved Changes", MessageBoxButtons.YesNo);
                if (window == DialogResult.No) e.Cancel = true;
                else e.Cancel = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // open RuleList help
            Process.Start("https://github.com/zumoshi/BrowserSelect/blob/master/help/filters.md");
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            //save rules

            //clear rules (instead of checking for changes we just overwrite the whole ruleset)
            Settings.Default.AutoBrowser.Clear();
            foreach (var rule in rules)
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
            btn_apply.Enabled = false;
            btn_cancel.Text = "Close";
        }

        private HelpRulesWindow _help;
        private void button1_Click(object sender, EventArgs e)
        {
            if (_help != null)
            {   //help window is open
                _help.Focus();
            }
            else
            {   //its not open...
                _help = new HelpRulesWindow();
                _help.FormClosed += (o, ev) => _help = null;
                _help.Show();
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            //close the window without saving or warning about unsaved changes
            btn_apply.Enabled = false;
            Close();
        }

        private void gv_filters_CellBeginEdit(object sender, EventArgs e)
        {
            //set the unsaved changes flag to true
            btn_apply.Enabled = true;
            btn_cancel.Text = "Cancel";
        }

        private void frm_settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            //close the help window (if it was open)
            _help?.Close();
        }

        private void btn_check_update_Click(object sender, EventArgs e)
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
        private void chk_check_update_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.check_update = (((CheckBox)sender).Checked) ? "0" : "nope";
            Settings.Default.Save();
        }
    }
}
