using BackupViewer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackupViewer.Classes
{
    class Update
    {
        string url = "https://docs.google.com/document/d/1GNKDb9jnC2k-DR3Sx82DzquxKAbV3xauHlKrRqWFQAs";
        string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool status = false;
        private List<string> data = new List<string>();

        UpdateInfo updateInfo = new UpdateInfo();

        public void CheckUpdate()
        {
            Log.AddLog("Checking for updates.");
            LoadGoogleDoc();
        }

        public event EventHandler UpdateСheckСompleted;
        private void UpdateСheckСompleted_()
        {
            if (UpdateСheckСompleted != null)
                UpdateСheckСompleted(updateInfo, EventArgs.Empty);
        }

        private void LoadGoogleDoc()
        {
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, p) =>
            {
                try 
                { 
                    GetVersion(p.Result);
                    status = true;
                }
                catch 
                {
                    status = false;
                    Log.AddLog("No Internet connection.");
                    UpdateСheckСompleted_();
                }
            };
            webClient.DownloadStringAsync(uri);
            
        }

        private void GetVersion(string p)
        {
            string finds = "";
            string text = p;
            //File.WriteAllText(@"D:\Users\ivan9\Desktop\my soft\C#\BackupViewer\BackupViewer\bin\Debug\data\ttt.txt", text);
            string pattern = "property=\"og:description\" content=\"(.*?)\"><";
            foreach (Match match in Regex.Matches(text, pattern))
            {
                finds = match.Groups[1].Value;
            }

            string pattern2 = "-separator-(.*?)-/separator-";
            foreach (Match match in Regex.Matches(finds, pattern2))
            {
                data.Add(match.Groups[1].Value);
            }

            updateInfo.Version = data[0].Replace(" ", "");
            var bytes = Encoding.Default.GetBytes(data[1]);
            updateInfo.Description = Encoding.UTF8.GetString(bytes);
            updateInfo.URL = data[2].Replace(" ", "");

            if (updateInfo.Version != currentVersion)
            {
                Log.AddLog("Update " + updateInfo.Version + " is available.");
                updateInfo.NeedToUpdate = true;
            }
            else
            {
                Log.AddLog("Update is not required.");
                updateInfo.NeedToUpdate = false;
            }
            UpdateСheckСompleted_();
        }
    }
}
