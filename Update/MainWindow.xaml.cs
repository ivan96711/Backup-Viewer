using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Update
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string currentVersion;
        string url = "https://docs.google.com/document/d/1GNKDb9jnC2k-DR3Sx82DzquxKAbV3xauHlKrRqWFQAs";
        string newVersion;
        string newVersionUrl;
        string path = Environment.CurrentDirectory + @"\BackupViewer.exe";
        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists(Environment.CurrentDirectory + @"data\old"))
                Directory.CreateDirectory(Environment.CurrentDirectory + @"data\old");

            UpdateStart();
        }

        private void UpdateStart()
        {
            _statusTextBox.Text = "Запуск";
            
            _statusTextBox.Text = "Проверка наличия обновлений";
                _statusProgressBar.Value = 20;

                if (File.Exists(path))
                    currentVersion = FileVersionInfo.GetVersionInfo(path).FileVersion;
                else
                    currentVersion = "0";
                LoadGoogleDoc();
        }

        private void LoadGoogleDoc()
        {
            Uri uri = new Uri(url);
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, p) =>
            {
                try
                {
                    _statusProgressBar.Value = 40;
                    GetVersion(p.Result);
                }
                catch (Exception e)
                {
                    _statusTextBox.Text = e.Message;
                    _statusProgressBar.Value = 100;
                }
            };
            webClient.DownloadStringAsync(uri);
        }

        
        private void GetVersion(string p)
        {
            string finds = "";
            string text = p;
            string pattern = "property=\"og:description\" content=\"(.*?)\"><";
            foreach (Match match in Regex.Matches(text, pattern))
            {
                finds = match.Groups[1].Value;
            }

            string pattern2 = "-separator-(.*?)-/separator-";
            List<string> data = new List<string>();
            foreach (Match match in Regex.Matches(finds, pattern2))
            {
                data.Add(match.Groups[1].Value);
            }

            newVersion = data[0].Replace(" ", "");
            newVersionUrl = data[2].Replace(" ", "").Replace("amp;", "");

            if (newVersion != currentVersion)
            {
                DownloadFile();
            }
            else
            {
                _statusTextBox.Text = "Обновление не требуется";
                _statusProgressBar.Value = 100;
                Process.Start(path);
                Environment.Exit(0);
            }
        }

        private void DownloadFile()
        {
            var procesQuery = from p in Process.GetProcesses()
                              where p.ProcessName == "BackupViewer"
                              select p;

            if (procesQuery.Count<Process>() > 0)
            {
                _statusTextBox.Text = "Запущен экземпляр программы, обновление невозможно";
                _statusProgressBar.Value = 100;
                return;
            }

            if (File.Exists(path))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\data\old\" + currentVersion);
                string backupPath = Environment.CurrentDirectory + @"\data\old\" + currentVersion + @"\BackupViewer.exe";
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(path, backupPath);
            }

            _statusTextBox.Text = "Скачивание";

            WebClient wc = new WebClient();
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(wc_DownloadProgressChanged);
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
            wc.DownloadFileAsync(new Uri(newVersionUrl), Environment.CurrentDirectory + @"\BackupViewer.exe");
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _statusProgressBar.Value = e.ProgressPercentage / 2 + 50;
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _statusTextBox.Text = "Завершение";

            Process.Start(path);
            Environment.Exit(0);
        }


    }
}
