using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackupViewer.Classes
{
    static class Log
    {
        private static readonly string LOG_FULLPATH = Environment.CurrentDirectory + @"\data\logfile.txt";
        private static Object theLock = new Object();
        public static ObservableCollection<string> log { get; set; } = new ObservableCollection<string>();

        public static void AddLog(string msg)
        {
            if (log.Count == 0)
                File.Delete(LOG_FULLPATH);

            log.Add(msg);
            lock (theLock)
            {
                File.AppendAllText(LOG_FULLPATH, string.Format("{0:G}: {1}{2}", DateTime.Now, msg, Environment.NewLine));
            }
        }
    }
}
