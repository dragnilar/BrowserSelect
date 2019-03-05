using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrowserSelect.Services;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BrowserSelect.DomainModels
{
    static class BrowserFinder
    {
        public static List<Browser> find()
        {
            List<Browser> browsers = new List<Browser>();
            //special case , firefox+firefox developer both installed
            //(only works if firefox installed in default directory)
            var ff_path = Path.Combine(
                Program.ProgramFilesx86(),
                @"Mozilla Firefox\firefox.exe");
            if (File.Exists(ff_path))
                browsers.Add(new Browser()
                {
                    BrowserName = "FireFox",
                    ExecutablePath = ff_path,
                    BrowserIcon = IconExtractor.fromFile(ff_path)
                });
            //special case , Edge
            var edge_path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\MicrosoftEdge.exe");
            if (File.Exists(edge_path))
                browsers.Add(new Browser()
                {
                    BrowserName = "Edge",
                    // #34
                    ExecutablePath = "shell:AppsFolder\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe!MicrosoftEdge",
                    BrowserIcon = IconExtractor.fromFile(edge_path)
                });

            //gather browsers from registry
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                browsers.AddRange(find(hklm));
            using (RegistryKey hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                browsers.AddRange(find(hkcu));

            //remove myself
            browsers = browsers.Where(x => Path.GetFileName(x.ExecutablePath).ToLower() !=
                                           Path.GetFileName(Application.ExecutablePath).ToLower()).ToList();
            //remove duplicates
            browsers = browsers.GroupBy(browser => browser.ExecutablePath)
                .Select(group => group.First()).ToList();
            //Check for Chrome Profiles
            Browser BrowserChrome = browsers.FirstOrDefault(x => x.BrowserName == "Google Chrome");
            if (BrowserChrome != null)
            {
                string ChromeUserDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data");
                List<string> ChromeProfiles = FindChromeProfiles(ChromeUserDataDir);

                if (ChromeProfiles.Count > 1)
                {
                    //add the Chrome instances and remove the default one
                    foreach (string Profile in ChromeProfiles)
                    {
                        browsers.Add(new Browser()
                        {
                            BrowserName = "Chrome (" + GetChromeProfileName(ChromeUserDataDir + "\\" + Profile) + ")",
                            ExecutablePath = BrowserChrome.ExecutablePath,
                            BrowserIcon = IconExtractor.fromFile(ChromeUserDataDir + "\\" + Profile + "\\Google Profile.ico"),
                            AdditionalArgs = String.Format("--profile-directory={0}", Profile)
                        });
                    }
                    browsers.Remove(BrowserChrome);
                    browsers = browsers.OrderBy(x => x.BrowserName).ToList();
                }
            }

            return browsers;
        }

        private static string GetChromeProfileName(string FullProfilePath)
        {
            dynamic ProfilePreferences = JObject.Parse(File.ReadAllText(FullProfilePath + @"\Preferences"));
            return ProfilePreferences.profile.name;
        }

        private static List<string> FindChromeProfiles(string ChromeUserDataDir)
        {
            List<string> Profiles = new List<string>();
            var ProfileDirs = Directory.GetFiles(ChromeUserDataDir, "Google Profile.ico", SearchOption.AllDirectories).Select(Path.GetDirectoryName);
            foreach (var Profile in ProfileDirs)
            {
                Profiles.Add(Profile.Substring(ChromeUserDataDir.Length + 1));
            }
            return Profiles;
        }

        private static List<Browser> find(RegistryKey hklm)
        {
            List<Browser> browsers = new List<Browser>();
            // startmenu internet key
            RegistryKey smi = hklm.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            if (smi != null)
                foreach (var browser in smi.GetSubKeyNames())
                {
                    try
                    {
                        var key = smi.OpenSubKey(browser);
                        var name = (string)key.GetValue(null);
                        var cmd = key.OpenSubKey("shell").OpenSubKey("open").OpenSubKey("command");
                        var exec = (string)cmd.GetValue(null);
                        

                        // by this point if registry is missing keys we are alreay out of here
                        // because of the try catch, but there are still things that could go wrong

                        //0. check if it can handle the http protocol
                        var capabilities = key.OpenSubKey("Capabilities");
                        // IE does not have the capabilities subkey...
                        // so assume that the app can handle http if it doesn't
                        // advertise it's capablities
                        if (capabilities != null)
                            if ((string)capabilities.OpenSubKey("URLAssociations").GetValue("http") == null)
                                continue;
                        //1. check if path is not empty
                        if (string.IsNullOrWhiteSpace(exec))
                            continue;

                        //1.1. remove possible "%1" from the end
                        exec = exec.Replace("\"%1\"", "");
                        //1.2. remove possible quotes around address
                        exec = exec.Trim("\"".ToCharArray());
                        //2. check if path is valid
                        if (!File.Exists(exec))
                            continue;
                        //3. check if name is valid
                        if (string.IsNullOrWhiteSpace(name))
                            name = Path.GetFileNameWithoutExtension(exec);

                        
                        browsers.Add(new Browser()
                        {
                            BrowserName = name,
                            ExecutablePath = exec,
                            BrowserIcon = IconExtractor.fromFile(exec),
                        });
                    }
                    catch (NullReferenceException)
                    {
                    } // incomplete registry record for browser, ignore it
                    catch (Exception ex)
                    {
                        // todo: log errors
                    }
                }
            return browsers;
        }
    }
}