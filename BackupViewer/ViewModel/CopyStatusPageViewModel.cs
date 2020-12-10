using BackupViewer.Classes;
using BackupViewer.Model;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupViewer.ViewModel
{
    class CopyStatusPageViewModel : BaseVM
    {
        private ObservableCollection<AddDataInfo> _CopyDataInfo;
        public ObservableCollection<AddDataInfo> CopyDataInfo
        {
            get { return _CopyDataInfo; }
            set
            {
                _CopyDataInfo = value;
                RaisePropertyChanged("CopyDataInfo");
            }
        }

        public CopyStatusPageViewModel()
        {
            //Привязываемся к событию OnConnectedChanged в CopyThread
            CopyThread.CollectionChanged += CollectionChanged;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollectionChanged(object sender, EventArgs e)
        {
            CopyDataInfo = new ObservableCollection<AddDataInfo>();
            CopyDataInfo = (ObservableCollection<AddDataInfo>)sender;
        }

        public ICommand Clear
        {
            get
            {
                return new DelegateCommand<int>((count) => {
                    CopyThread.СlearingСopiedData();
                }, (count) => count > 0);
            }
        }
    }
}
