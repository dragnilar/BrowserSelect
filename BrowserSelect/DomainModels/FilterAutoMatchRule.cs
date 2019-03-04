using System;

namespace BrowserSelect.DomainModels
{
    class FilterAutoMatchRule
    {
        public string Pattern { get; set; }
        public string Browser { get; set; }

        public static implicit operator FilterAutoMatchRule(System.String s)
        {
            var ss = s.Split(new[] { "[#!][$~][?_]" }, StringSplitOptions.None);
            return new FilterAutoMatchRule()
            {
                Pattern = ss[0],
                Browser = ss[1]
            };
        }

        public override string ToString()
        {
            return Pattern + "[#!][$~][?_]" + Browser;
        }

        public string error()
        {
            if (!string.IsNullOrEmpty(Pattern))
                return string.Format("You forgot to select a Browser for '{0}' rule.", Pattern);
            else if (!string.IsNullOrEmpty(Browser))
                return "one of your rules has an Empty pattern. please refer to Help for more information.";
            else
                return "";
        }

        public bool valid()
        {
            try //because they may be null
            {
                return Browser.Length > 0 && Pattern.Length > 0;
            }
            catch
            {
                return false;
            }

        }
    }
}