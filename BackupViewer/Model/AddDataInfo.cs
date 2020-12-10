using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.Model
{
    public class AddDataInfo : BaseVM
    {
        /// <summary>
        /// Имя копии
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Директория копируемого элемента
        /// </summary>
        public ObservableCollection<Directory> Directories { get; set; } = new ObservableCollection<Directory>();
        /// <summary>
        /// Директория исполняемого файла
        /// </summary>
        public string ExecutableFileDirectory { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Категории
        /// </summary>
        public ObservableCollection<CategoryItem> Category { get; set; } = new ObservableCollection<CategoryItem>();
        /// <summary>
        /// Изображения
        /// </summary>
        public ObservableCollection<Directory> Images { get; set; } = new ObservableCollection<Directory>();

        /// <summary>
        /// Статус копирования
        /// 0 - не скопировано
        /// 1 - в процессе копирования/копирование не завершено
        /// 2 - копирование завершено
        /// </summary>
        public byte CopyStatus { get; set; }

        public string CopyStatusText { get; set; }
    }
}
