using BackupViewer.Classes;
using BackupViewer.Model;
using BackupViewer.View;
using DevExpress.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BackupViewer.ViewModel
{
    class MainPageViewModel : BaseVM
    {
        //Все данные из json, полученные от MainWindowViewModel
        public ObservableCollection<AddDataInfo> allDataInfo { get; set; }
        public ICollectionView DisplayAllDataInfo { get; set; }
        //Все используемые категории, полученные от MainWindowViewModel 
        public ObservableCollection<SelectedCategories> allСategories { get; set; }
        public ObservableCollection<ImageSource> ImagePath { get; set; }

        //Подстраницы

        private Page ViewPage = new MainPageMainSlide();

        private Page EditPage = new MainPageEditSlide();
        private Page _currentMainPage;
        public Page CurrentMainPage
        {
            get { return _currentMainPage; }
            set
            {
                if (_currentMainPage == value)
                    return;

                _currentMainPage = value;
                RaisePropertyChanged("CurrentMainPage");
            }
        }


        //Выбранный элемент в listbox
        private AddDataInfo _SelectedData { get; set; }
        public AddDataInfo SelectedData
        {
            get { return _SelectedData; }
            set
            {
                if (value != null)
                {
                    Log.AddLog("Selected " + value.Name);
                    _SelectedData = value;
                    ImagePath = new ObservableCollection<ImageSource>();
                    if (SelectedData.Images != null && SelectedData.Images.Count > 0)
                        foreach (var img in SelectedData.Images)
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = new Uri(img.Path_);
                            image.EndInit();

                            ImagePath.Add(image);
                        }

                    ShowMainPageMainSlide();

                    RaisePropertyChanged("ImagePath");
                    RaisePropertyChanged("SelectedData");
                }
            }
        }

        private void ShowMainPageMainSlide()
        {
            MainPageMainSlideViewModel vm = new MainPageMainSlideViewModel
            {
                SelectedData = SelectedData,
                allDataInfo = allDataInfo
            };
            vm.SearchKeywordNotify += DisplayNewSearchKeyword;
            vm.DeleteSelectedNotify += DeleteSelected;
            ViewPage.DataContext = vm;
            CurrentMainPage = ViewPage;
        }
        private void HideFrame()
        {
            CurrentMainPage = null;
        }

        private void DisplayNewSearchKeyword(string SearchKeywordNew)
        {
            SearchKeyword = SearchKeywordNew;
            RaisePropertyChanged("SearchKeyword");
        }

        private void DeleteSelected()
        {
            Log.AddLog("Removing an item");
            string rootDir = Environment.CurrentDirectory + @"\data\copydata\" + SelectedData.Name;
            bool continueDeleting = false;

            switch (System.IO.Directory.Exists(rootDir))
            {
                case true:
                    try
                    {
                        System.IO.Directory.Delete(rootDir, true);
                    }
                    catch (Exception e)
                    {
                        Log.AddLog(e.Message);
                        MessageBox.Show(e.Message);
                    }
                    continueDeleting = true;
                    break;

                case false:
                    Log.AddLog("Directory of the selected item does not exist");
                    MessageBoxResult result = MessageBox.Show("Указанная директория не существует" + '\n' + "Удалить упоминание \"" + SelectedData.Name + "\" из базы?", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                        continueDeleting = true;
                    break;
            }

            if (continueDeleting != true)
                return;
            allDataInfo.Remove(SelectedData);
            Log.AddLog("Removing " + SelectedData.Name + " from json");

            DisplayAllDataInfo = CollectionViewSource.GetDefaultView(allDataInfo);
            SelectedData = allDataInfo.FirstOrDefault();
            if (allDataInfo.Count == 0)
                HideFrame();
            Log.AddLog('\t' + "Saving json");
            FixDirectory fixDirectory = new FixDirectory();
            File.WriteAllText(@"data\BackupData.json", JsonConvert.SerializeObject(fixDirectory.CorrectDirectoryToSave(allDataInfo)));
            allDataInfo = fixDirectory.CorrectDirectoryToWork(allDataInfo);
            Log.AddLog("done.");

            ShowMainPageMainSlide();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Поиск среди имен
        /// </summary>
        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                
                DisplayAllDataInfo.Filter = (obj) =>
                {
                    if (obj is AddDataInfo data)
                    {
                        if (SearchKeyword == null)
                            SearchKeyword = "";

                        switch (SearchKeyword.FirstOrDefault())
                        {
                            case '#': return data.Category.FirstOrDefault(s => s.Value.ToLower().Contains(SearchKeyword.Remove(0, 1).ToLower())) != null;

                            default: return data.Name.ToLower().Contains(SearchKeyword.ToLower());
                        }
                    }

                    return false;
                };
                DisplayAllDataInfo.Refresh();
            }
        }

        public ICommand CategoryClick
        {
            get
            {
                return new DelegateCommand<string>((category) =>
                {
                    if (category != null)
                    {
                        SearchKeyword = "#" + category;
                        RaisePropertyChanged("SearchKeyword");
                    }
                });
            }
        }

        public MainPageViewModel()
        {
            DisplayAllDataInfo = CollectionViewSource.GetDefaultView(allDataInfo);            
        }

        
    }
}
