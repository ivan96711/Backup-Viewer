using BackupViewer.Classes;
using BackupViewer.Model;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BackupViewer.ViewModel
{
    class MainPageMainSlideViewModel : BaseVM
    {
        public ObservableCollection<AddDataInfo> allDataInfo { get; set; }
        public AddDataInfo SelectedData { get; set; }
        public string SearchKeyword { get; set; }
        public delegate void SearchKeywordHandler(string SearchKeyword);
        public event SearchKeywordHandler SearchKeywordNotify;

        public ICommand CategoryClick
        {
            get
            {
                return new DelegateCommand<string>((category) =>
                {
                    if (category != null)
                    {
                        SearchKeyword = "#" + category;
                        SearchKeywordNotify?.Invoke(SearchKeyword);
                        RaisePropertyChanged("SearchKeyword");
                    }
                });
            }
        }
        
        public ICommand OpenFolder
        {
            get
            {
                return new DelegateCommand<AddDataInfo>((s) =>
                {
                    string rootDir = Environment.CurrentDirectory + @"\data\copydata\" + SelectedData.Name + @"\data";
                    Log.AddLog("Open directory: " + rootDir);
                    try
                    {
                        Process.Start(rootDir);
                    }
                    catch (Exception e)
                    {
                        Log.AddLog(e.Message);
                        MessageBox.Show(e.Message);
                    }
                }, (s) => s != null);
            }
        }

        public delegate void DeleteSelectedHandler();
        public event DeleteSelectedHandler DeleteSelectedNotify;
        public ICommand DeleteSelected
        {
            get
            {
                return new DelegateCommand<AddDataInfo>((s) =>
                {
                    DeleteSelectedNotify?.Invoke();
                }, (s) => s != null);
            }
        }

        
    }
}
