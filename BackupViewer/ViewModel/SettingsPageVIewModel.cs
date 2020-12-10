using BackupViewer.Classes;
using BackupViewer.Model;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.ViewModel
{
    class SettingsPageVIewModel : BaseVM
    {
        public string currentVersion { get; set; }
        public UpdateInfo updateInfo { get; set; } = new UpdateInfo();
        public string newVersion { get; set; }
        public bool visibilityUpdate { get; set; } = false;

        public SettingsPageVIewModel()
        {
            currentVersion = "Текущая версия " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public DelegateCommand CheckUpdate
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Update update = new Update();
                    update.UpdateСheckСompleted += UpdateСheckСompleted;
                    update.CheckUpdate();                    
                });
            }
        }

        public DelegateCommand Update
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Process.Start(Environment.CurrentDirectory + @"\data\Update.exe");
                    Environment.Exit(0);
                });
            }
        }

        private void UpdateСheckСompleted(object sender, EventArgs e)
        {
            updateInfo = (UpdateInfo)sender;
            if (updateInfo.Version != null && updateInfo.NeedToUpdate)
            {
                newVersion = "Доступна версия " + updateInfo.Version;
                visibilityUpdate = true;
                RaisePropertyChanged("visibilityUpdate");
                RaisePropertyChanged("newVersion");
                currentVersion = "Текущая версия " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                RaisePropertyChanged("currentVersion");
            }
            if (updateInfo.Version != null && !updateInfo.NeedToUpdate)
            {
                currentVersion = "Текущая версия " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + '\n' + "Обновление не требуется";
                RaisePropertyChanged("currentVersion");
                visibilityUpdate = false;
                RaisePropertyChanged("visibilityUpdate");
            }
        }
    }
}
