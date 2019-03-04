using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using DevExpress.XtraEditors.Controls;

namespace BrowserSelect.DomainModels
{
    public class Browser
    {
        public string name;
        public string exec;
        public Icon icon;
        public string additionalArgs = "";

        public string private_arg
        {
            get
            {
                var file = exec.Split(new[] { '/', '\\' }).Last().ToLower();
                if (file.Contains("chrome") || file.Contains("chromium"))
                    return "-incognito";
                if (file.Contains("opera"))
                    return "-newprivatetab";
                if (file.Contains("iexplore"))
                    return "-private";
                if (file.Contains("edge"))
                    return "-private";
                if (file.Contains("launcher"))
                    return "-private";
                return "-private-window";  // FF
            }
        }

        public List<char> shortcuts => Regex.Replace(name, @"[^A-Za-z\s]", "").Split(' ')
            .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Substring(0, 1).ToLower()[0]).ToList();
        public override string ToString()
        {
            return name;
        }
        public static implicit operator Browser(string s)
        {
            return BrowserFinder.find().First(b => b.name == s);
        }

        public static explicit operator Browser(CheckedListBoxItem v)
        {
            throw new NotImplementedException();
        }
    }
}
