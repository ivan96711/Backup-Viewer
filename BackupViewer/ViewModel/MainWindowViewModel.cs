using DevExpress.Mvvm;
using BackupViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using BackupViewer.View;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using BackupViewer.Classes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace BackupViewer.ViewModel
{
    class MainWindowViewModel : BaseVM
    {
        /// <summary>
        /// Все данные
        /// </summary>
        public ObservableCollection<AddDataInfo> allDataInfo { get; set; }
        bool canUpdateAllDataInfo = true;
        /// <summary>
        /// Список всех категорий
        /// </summary>
        public ObservableCollection<SelectedCategories> allСategories { get; set; }
        /// <summary>
        /// Выбранный эелемент на странице AddPage
        /// </summary>
        public AddDataInfo SelectedData { get; set; }
        /// <summary>
        /// Информация о доступном обновлении
        /// </summary>
        UpdateInfo updateInfo { get; set; } = new UpdateInfo();

        public string PageName { get; set; }

        private Page MainPage = new MainPage();

        private Page AddPage = new AddPage();

        private Page CopyStatusPage = new CopyStatusPage();
        private Page SettingsPage = new SettingsPage();

        private Page _currentPage;
        public Page CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage == value)
                    return;

                _currentPage = value;
                RaisePropertyChanged("CurrentPage");
            }
        }

        FixDirectory fixDirectory = new FixDirectory();
        Category Category = new Category();

        public MainWindowViewModel()
        {
            Update update = new Update();
            //Привязываемся к событию OnConnectedChanged в CopyThread для отлова завершенных процессов
            update.UpdateСheckСompleted += UpdateСheckСompleted;
            update.CheckUpdate();            

            //запуск уведовлений
            Toast.ToastInitialization();

            //загружаем json
            allDataInfo = fixDirectory.CorrectDirectoryToWork(File.Exists(@"data\BackupData.json") ? JsonConvert.DeserializeObject<ObservableCollection<AddDataInfo>>(File.ReadAllText(@"data\BackupData.json")) : new ObservableCollection<AddDataInfo>());
            allDataInfo.CollectionChanged += (s, e) =>
            {
                if (canUpdateAllDataInfo)
                {
                    Log.AddLog("allDataInfo has been updated.");

                    allСategories = Category.GetCategories(allDataInfo);
                }
            };

            CopyThread.CopyInitialization();

            allСategories = Category.GetCategories(allDataInfo);


            //Привязываемся к событию OnConnectedChanged в CopyThread для отлова завершенных процессов
            CopyThread.CopiedDataCollectionChanged += CollectionChanged;

            //Открываем первую страницу и выделяем первый элемент
            SelectedData = allDataInfo.FirstOrDefault();
            var vm = new MainPageViewModel
            {
                SelectedData = allDataInfo.FirstOrDefault(),
                allDataInfo = allDataInfo,
                allСategories = allСategories,
                DisplayAllDataInfo = CollectionViewSource.GetDefaultView(allDataInfo)
        };
            MainPage.DataContext = vm;
            PageName = "Просмотр";
            CurrentPage = MainPage;

            RaisePropertyChanged("PageName");
        }

        private void UpdateСheckСompleted(object sender, EventArgs e)
        {
            updateInfo = (UpdateInfo)sender;
            if(updateInfo.NeedToUpdate)
                Toast.ShowInformation("Доступно обновение " + updateInfo.Version);
        }

        private void CollectionChanged(object sender, EventArgs e)
        {
            var newData = (AddDataInfo)sender;

            var item = allDataInfo.FirstOrDefault(i => i == newData);

            if (item == null)
            {
                allDataInfo.Add(newData);
                Log.AddLog('\t' + "Saving json");
                File.WriteAllText(@"data\BackupData.json", JsonConvert.SerializeObject(fixDirectory.CorrectDirectoryToSave(allDataInfo)));
                allDataInfo = fixDirectory.CorrectDirectoryToWork(allDataInfo);
            }
        }        

        public ICommand MainMenuClick
        {
            get
            {
                return new DelegateCommand(() => {
                    PageName = "Просмотр";
                    RaisePropertyChanged("PageName");

                    Log.AddLog("Open MainPage.");
                    SelectedData = allDataInfo.FirstOrDefault();
                    var vm = new MainPageViewModel
                    {
                        SelectedData = allDataInfo.FirstOrDefault(),
                        allDataInfo = allDataInfo,
                        allСategories = allСategories,
                        DisplayAllDataInfo = CollectionViewSource.GetDefaultView(allDataInfo)
                    };
                    RaisePropertyChanged("visibilityMainPage");
                    MainPage.DataContext = vm;
                    CurrentPage = MainPage;
                });
            }
        }

        public ICommand AddMenuClick
        {
            get
            {
                return new DelegateCommand(() => {
                    PageName = "Добавить";
                    RaisePropertyChanged("PageName");
                    Log.AddLog("Open AddPage.");

                    var vm = new AddPageViewModel
                    {
                        allDataInfo = allDataInfo,
                        allСategories = allСategories
                    };
                    AddPage.DataContext = vm;
                    CurrentPage = AddPage;
                    
                });
            }
        }

        public ICommand CopyStatus
        {
            get
            {
                return new DelegateCommand(() => {
                    PageName = "Очередь на копирование";
                    RaisePropertyChanged("PageName");
                    Log.AddLog("Open CopyStatusPage.");
                    CurrentPage = CopyStatusPage;
                });
            }
        }

        public ICommand Settings
        {
            get
            {
                return new DelegateCommand(() => {
                    PageName = "Настройки";
                    RaisePropertyChanged("PageName");
                    Log.AddLog("Open SettingsPage.");
                    var vm = new SettingsPageVIewModel
                    {
                        updateInfo = updateInfo
                    };
                    SettingsPage.DataContext = vm;
                    CurrentPage = SettingsPage;
                });
            }
        }
    }
}
