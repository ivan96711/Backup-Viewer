using BackupViewer.Classes;
using BackupViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackupViewer.ViewModel
{
    class LogViewModel : BaseVM
    {
        public ObservableCollection<string> logs { get; set; }

        public LogViewModel()
        {
            logs = new ObservableCollection<string>();
            Thread myThread = new Thread(new ThreadStart(Check));
            myThread.Start();
        }

        public void Check()
        {
            while (true)
            {
                logs = Log.log;

                Thread.Sleep(50);

                RaisePropertyChanged("logs");
            }
        }
    }
}
