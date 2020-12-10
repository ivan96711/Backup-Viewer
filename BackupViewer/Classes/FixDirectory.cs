using BackupViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.Classes
{
    class FixDirectory
    {
        /// <summary>
        /// Удаление пути до папки data для всех директорий json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ObservableCollection<AddDataInfo> CorrectDirectoryToSave(ObservableCollection<AddDataInfo> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Directories != null)
                    for (int f = 0; f < data[i].Directories.Count; f++)
                        data[i].Directories[f].Path_ = data[i].Directories[f].Path_.Replace(Environment.CurrentDirectory, "");
                if (data[i].Images != null)
                    for (int f = 0; f < data[i].Images.Count; f++)
                        data[i].Images[f].Path_ = data[i].Images[f].Path_.Replace(Environment.CurrentDirectory, "");
            }
            return data;
        }

        /// <summary>
        /// Добавление пути до папки data для всех директорий json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ObservableCollection<AddDataInfo> CorrectDirectoryToWork(ObservableCollection<AddDataInfo> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Directories != null)
                    for (int f = 0; f < data[i].Directories.Count; f++)
                        data[i].Directories[f].Path_ = Environment.CurrentDirectory + data[i].Directories[f].Path_.Replace(Environment.CurrentDirectory, "");
                if (data[i].Images != null)
                    for (int f = 0; f < data[i].Images.Count; f++)
                        data[i].Images[f].Path_ = Environment.CurrentDirectory + data[i].Images[f].Path_.Replace(Environment.CurrentDirectory, "");
            }
            return data;
        }
    }
}
