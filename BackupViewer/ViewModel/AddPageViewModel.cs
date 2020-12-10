using BackupViewer.Classes;
using BackupViewer.Model;
using BackupViewer.View;
using DevExpress.Mvvm;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace BackupViewer.ViewModel
{
    class AddPageViewModel : BaseVM, IDropTarget
    {
        //Все данные из json, полученные от MainWindowViewModel
        public ObservableCollection<AddDataInfo> allDataInfo { get; set; }
        //Все используемые категории, полученные от MainWindowViewModel 
        public ObservableCollection<SelectedCategories> allСategories { get; set; }
        //Вся вбиваемая пользователем информация в данном окне
        public AddDataInfo dataInfo { get; set; } = new AddDataInfo();
        //Отображение табличек DragAndDrop
        public bool visibilityDragAndDrop { get; set; } = true;
        public bool visibilityDragAndDropImage { get; set; } = true;

        public bool SomeItemSelected { get; set; }

        public bool visibilityNameDuplicate { get; set; } = false;
        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                visibilityNameDuplicate = false;
                foreach (var data in allDataInfo)
                {
                    if (data.Name.ToLower() == value.ToLower())
                        visibilityNameDuplicate = true;
                }
                foreach (var data in CopyThread.notCopiedData)
                {
                    if (data.Name.ToLower() == value.ToLower())
                        visibilityNameDuplicate = true;
                }
                RaisePropertyChanged("visibilityNameDuplicate");
                dataInfo.Name = value;
            }
        }

        public AddPageViewModel()
        {
        }
        
        /// <summary>
        /// Кнопка сохранить
        /// Параметры:
        ///     dataInfo != null
        ///     info.Name != null && info.Name != "" - свойство Name задано и не пустое
        ///     info.Directories.Count - добавлены файлы для копирования
        /// </summary>
        public ICommand Save
        {
            get
            {
                return new DelegateCommand<AddDataInfo>((info) =>
                {
                    Log.AddLog("Saving");
                    Log.AddLog('\t' + "Name: " + dataInfo.Name);
                    Log.AddLog('\t' + "Description: " + dataInfo.Description);
                    Log.AddLog('\t' + "ExecutableFile: " + dataInfo.ExecutableFileDirectory);
                    Log.AddLog('\t' + "Files:");
                    foreach (var s in dataInfo.Directories)
                        Log.AddLog('\t' + "\t" + s.Path_);
                    Log.AddLog('\t' + "Images:");
                    foreach (var s in dataInfo.Images)
                        Log.AddLog('\t' + "\t" + s);
                    Log.AddLog('\t' + "Categories:");
                    foreach (var s in dataInfo.Category)
                        Log.AddLog('\t' + "\t" + s.Value);

                    //собираем все категории в кучу
                    Category category = new Category();
                    dataInfo.Category = category.RemoveDuplicates(dataInfo.Category, allСategories);

                    //устанавливаем статус 0 - не скопировано
                    dataInfo.CopyStatus = 0;
                    dataInfo.CopyStatusText = "Ожидание";

                    //добавляем новый элемент в очередь
                    Log.AddLog('\t' + "Added");
                    if (CopyThread.copyInProcess)
                        Toast.ShowInformation(dataInfo.Name + " в очереди на копирование");
                    CopyThread.notCopiedData.Add(dataInfo);

                    //сбрасываем все данные на текущей странице
                    allСategories = category.GetCategories(allDataInfo);
                    allСategories = category.AddNewCategories(dataInfo.Category, allСategories);
                    RaisePropertyChanged("allСategories");
                    dataInfo = new AddDataInfo();
                    RaisePropertyChanged("dataInfo");
                    Name = "";
                    RaisePropertyChanged("Name");
                    visibilityDragAndDrop = true;
                    RaisePropertyChanged("visibilityDragAndDrop");

                    for(int i = 0; i < allСategories.Count; i++)
                    {
                        if (!allСategories[i].SomeItemSelected)
                            allСategories[i].SomeItemSelected = false;
                    }
                    
                    foreach(var cat in dataInfo.Category)
                    {
                        allСategories.Add(new SelectedCategories
                        {
                            Name = cat.Value,
                            SomeItemSelected = false
                        });
                    }

                }, (info) => info != null && info.Name != null && info.Name != "" && info.Directories.Count != 0 && !visibilityNameDuplicate);
            }
        }

        /// <summary>
        /// Добавляем новую категорию с пустрой строкой
        /// </summary>
        public DelegateCommand AddCategory
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Log.AddLog("Added a category.");
                    dataInfo.Category.Add(new CategoryItem { Value = "" });
                });
            }
        }

        /// <summary>
        /// Отработка удаления файла из списка на копирование
        /// </summary>
        public DelegateCommand<Model.Directory> DeleteDir
        {
            get
            {
                return new DelegateCommand<Model.Directory>((keyword) =>
                {
                    if (keyword != null)
                    {
                        Log.AddLog("Removed directory: " + keyword.Path_);
                        dataInfo.Directories.Remove(keyword);
                        if (dataInfo.Directories.Count == 0)
                        {
                            visibilityDragAndDrop = true;
                            RaisePropertyChanged("visibilityDragAndDrop");
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Отработка удаления новой категории
        /// </summary>
        public DelegateCommand<CategoryItem> DeleteCategory
        {
            get
            {
                return new DelegateCommand<CategoryItem>((keyword) =>
                {
                    if (keyword != null)
                    {
                        Log.AddLog("Removed category: " + keyword.Value);
                        dataInfo.Category.Remove(keyword);
                    }
                });
            }
        }

        /// <summary>
        /// Отработка удаления изображений из списка на копирование
        /// </summary>
        public DelegateCommand<Model.Directory> DeleteImage
        {
            get
            {
                return new DelegateCommand<Model.Directory>((keyword) =>
                {
                    if (keyword != null)
                    {
                        Log.AddLog("Removed image: " + keyword.Path_);
                        dataInfo.Images.Remove(keyword);
                        if (dataInfo.Images.Count == 0)
                        {
                            visibilityDragAndDropImage = true;
                            RaisePropertyChanged("visibilityDragAndDropImage");
                        }
                    }
                });
            }
        }

        #region Drag and drop
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var dragFileList = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = dragFileList.Any(item =>
            {
                var extension = Path.GetExtension(item);
                return extension != null;
            }) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var data = ((DataObject)dropInfo.Data).GetFileDropList();

            if(dropInfo.VisualTargetItem.Uid == "888")
                foreach (string item in data)
                {
                    //проверка на дубликаты

                    bool g = true;

                    for (int i = 0; i < dataInfo.Directories.Count; i++)
                    {
                        if (dataInfo.Directories[i].Path_ == item)
                            g = false;
                    }

                    if (g)
                    {
                        Log.AddLog("File added: " + item);
                        dataInfo.Directories.Add(new Model.Directory
                        {
                            Path_ = item,
                            Extension = Path.GetExtension(item)
                        });
                        visibilityDragAndDrop = false;
                        RaisePropertyChanged("visibilityDragAndDrop");
                    }
                }

            if (dropInfo.VisualTargetItem.Uid == "999")
                foreach (string item in data)
                {
                    bool asd = false;
                    string[] formats = new string[] {".ani", ".clp", ".cmp", ".cmw", ".cur", ".dic", ".enf", ".exif", ".flc", ".gif", ".hdp", 
                        ".ico", ".iff", ".jpg", ".jp2", ".pbm", ".pcx", ".png", ".psd", ".ras", ".sgi", ".tga", ".tiff", ".wmf", ".wpg", ".xpm",
                        ".xwd", ".abc", ".cal", ".cmp", ".ica", ".img", ".itg", ".jb2", ".mac", ".xps", ".mng", ".mrc", ".msp", ".smp", ".tiff",
                        ".emf", ".xbm", ".xps", ".bmp"};
                    string format = Path.GetExtension(item).ToLower();

                    foreach (string f in formats)
                    {
                        if (f == format)
                        {
                            asd = true;
                            break;
                        }
                    }

                    //проверка формата
                    if (asd)
                    {

                        //проверка на дубликаты
                        bool g = true;

                        for (int i = 0; i < dataInfo.Images.Count; i++)
                        {
                            if (dataInfo.Images[i].Path_ == item)
                                g = false;
                        }

                        if (g)
                        {
                            Log.AddLog("Image added: " + item);

                            dataInfo.Images.Add(new Model.Directory
                            {
                                Path_ = item,
                                Extension = Path.GetExtension(item)
                            });
                            visibilityDragAndDropImage = false;
                            RaisePropertyChanged("visibilityDragAndDropImage");
                        }
                    }
                }
        }
        #endregion
    }
}
