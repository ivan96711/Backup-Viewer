using BackupViewer.Model;
using BackupViewer.View;
using BackupViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ToastNotifications.Core;

namespace BackupViewer.Classes
{
    static class CopyThread
    {
        /// <summary>
        /// Нескопированные элементы
        /// </summary>
        public static ObservableCollection<AddDataInfo> notCopiedData { get; set; } = new ObservableCollection<AddDataInfo>();
        /// <summary>
        /// Скопированные элементы
        /// </summary>
        //public static ObservableCollection<AddDataInfo> CopiedData { get; set; } = new ObservableCollection<AddDataInfo>();
        /// <summary>
        /// Запрет на вызов CollectionChanged
        /// </summary>
        private static bool canUpdateNotCopiedData = true;
        /// <summary>
        /// Флаг для проверки идет копирование или нет
        /// </summary>
        public static bool copyInProcess = false;
        /// <summary>
        /// AddDataInfo элемента, который находится в процессе копирования
        /// </summary>
        private static AddDataInfo copiedDataInProcess;
        /// <summary>
        /// Процент завершения копирования
        /// </summary>
        public static int copyPercent = 0;
        /// <summary>
        /// Порядковый номер копируемого элемента в notCopiedData
        /// </summary>
        private static int id;
        /// <summary>
        /// Флаг, который допускает завершение копирования
        /// </summary>
        private static bool canEndCopying = true;
        
        public static void CopyInitialization()
        {           
            notCopiedData.CollectionChanged += (s, e) =>
            {
                if (canUpdateNotCopiedData)
                {
                    OnCollectionChanged();
                    StartCopyThread();
                }
            };

            //CopiedData.CollectionChanged += (s, e) =>
            //{
            //    CopiedDataOnCollectionChanged();
            //};
        }

        public static event EventHandler CollectionChanged;
        private static void OnCollectionChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged(notCopiedData, EventArgs.Empty);
        }

        public static event EventHandler CopiedDataCollectionChanged;
        private static void CopiedDataOnCollectionChanged(AddDataInfo add)
        {
            if (CopiedDataCollectionChanged != null)
                CopiedDataCollectionChanged(add, EventArgs.Empty);
        }

        /// <summary>
        /// Запуск потока копирования
        /// 1. Сначала идет проверка, находится ли другой поток в процессе (copyInProcess)
        /// 2. В цикле ищем первый попавшийся элемент с CopyStatus == 0
        /// 3. Запускаем его процесс
        /// </summary>
        private static void StartCopyThread()
        {
            //проверка, идет ли копирование
            if (!copyInProcess)
                for (int i = 0; i < notCopiedData.Count(); i++)
                {
                    if (notCopiedData[i].CopyStatus == 0)
                    {
                        copiedDataInProcess = notCopiedData[i];

                        id = i;

                        canUpdateNotCopiedData = false;
                        notCopiedData[id].CopyStatus = 1;
                        notCopiedData[id].CopyStatusText = "Идет копирование";
                        canUpdateNotCopiedData = true;
                        Toast.ShowInformation("Началось копирование " + notCopiedData[id].Name);

                        copyInProcess = true;

                        BackgroundWorker worker = new BackgroundWorker();
                        worker.WorkerReportsProgress = true;
                        worker.DoWork += worker_DoWork;
                        worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                        worker.ProgressChanged += worker_ProgressChanged;
                        worker.RunWorkerAsync();

                        break;
                    }
                }
        }

        /// <summary>
        /// Выводим завершение копирования в процентах
        /// </summary>
        static void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            copyPercent = e.ProgressPercentage;
        }

        /// <summary>
        /// По завершении копировапния меняем CopyStatus на 2 и записываем его в copiedDataInProcess
        /// Меняем copyInProcess на false для разрешения дальнейшего копирования
        /// </summary> 
        static void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CopiedCompleted();
        }

        //Вынесено отдельно для избежания ситуации, когда идет чистка списка (СlearingСopiedData) и одновременно заканчивается копирование
        private static void CopiedCompleted()
        {
            if (canEndCopying)
            {
                copiedDataInProcess.CopyStatus = 2;
                copiedDataInProcess.CopyStatusText = "Готово";
                Toast.ShowSuccess("Копирование " + notCopiedData[id].Name + " завершено");
                CopiedDataOnCollectionChanged(copiedDataInProcess);

                copyInProcess = false;

                notCopiedData[id] = copiedDataInProcess;

                Log.AddLog("done.");
            }
            else
            {
                Thread.Sleep(10);
                CopiedCompleted();
            }
        }

        private static string rootDir;
        private static string dataFolder;
        private static string imageFolder;
        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Log.AddLog("Copying has started");
            CreateFolders();


            Copying(sender);
        }

        private static void CreateFolders()
        {
            try
            {
                Log.AddLog('\t' + "Create folders...");

                if (System.IO.Directory.Exists(Environment.CurrentDirectory + @"\data\copydata"))
                    System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + @"\data\copydata");

                rootDir = Environment.CurrentDirectory + @"\data\copydata\" + copiedDataInProcess.Name;
                dataFolder = rootDir + @"\data";
                imageFolder = rootDir + @"\image";

                System.IO.Directory.CreateDirectory(rootDir);
                System.IO.Directory.CreateDirectory(imageFolder);
                System.IO.Directory.CreateDirectory(dataFolder);

                Log.AddLog('\t' + "done.");
            }
            catch (Exception e)
            {
                Log.AddLog(e.Message);
                MessageBox.Show(e.Message);
            }
        }

        private static void Copying(object sender)
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024];
                int currentBlockSize = 0;
                long allFileCopy = 0;

                for (int i = 0; i < copiedDataInProcess.Directories.Count; i++)
                {
                    string from = copiedDataInProcess.Directories[i].Path_;
                    string name = new DirectoryInfo(from).Name;
                    string to = dataFolder + @"\" + name;
                    copiedDataInProcess.Directories[i].Path_ = to;

                    if (File.GetAttributes(from).HasFlag(FileAttributes.Directory))
                    {
                        Log.AddLog('\t' + "Folder copying started: " + from);

                        System.IO.Directory.CreateDirectory(to);

                        foreach (string dirPath in System.IO.Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
                            System.IO.Directory.CreateDirectory(dirPath.Replace(from, to));

                        foreach (string file in System.IO.Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
                        {
                            Log.AddLog('\t' + "\tFile copying: " + file);
                            using (FileStream source = new FileStream(file, FileMode.Open, FileAccess.Read))
                            {
                                string filik = file.Substring(file.LastIndexOf('\\'), file.Length - file.LastIndexOf('\\'));
                                long fileLength = source.Length;
                                using (FileStream dest = new FileStream(file.Replace(from, to), FileMode.CreateNew, FileAccess.Write))
                                {
                                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        allFileCopy += currentBlockSize;
                                        dest.Write(buffer, 0, currentBlockSize);
                                        (sender as BackgroundWorker).ReportProgress(Convert.ToInt32(allFileCopy * 100.0 / fileLength));
                                    }
                                }
                            }
                        }

                        Log.AddLog('\t' + "done.");
                    }
                    else
                    {
                        Log.AddLog('\t' + "File copying started: " + from);

                        using (FileStream source = new FileStream(from, FileMode.Open, FileAccess.Read))
                        {
                            string filik = from.Substring(from.LastIndexOf('\\'), from.Length - from.LastIndexOf('\\'));
                            long fileLength = source.Length;
                            using (FileStream dest = new FileStream(from.Replace(from, to), FileMode.CreateNew, FileAccess.Write))
                            {
                                while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    allFileCopy += currentBlockSize;
                                    dest.Write(buffer, 0, currentBlockSize);
                                    (sender as BackgroundWorker).ReportProgress(Convert.ToInt32(allFileCopy * 100.0 / fileLength));
                                }
                            }
                        }

                        Log.AddLog("    done.");
                    }

                    
                }

                for(int i = 0; i < copiedDataInProcess.Images.Count; i++)
                {
                    string from = copiedDataInProcess.Images[i].Path_;
                    string to = imageFolder + @"\" + copiedDataInProcess.Name + "_" + i.ToString() + copiedDataInProcess.Images[i].Extension;
                    File.Copy(from, to);

                    canUpdateNotCopiedData = false;
                    copiedDataInProcess.Images[i].Path_ = to.Replace(Environment.CurrentDirectory, "");
                    canUpdateNotCopiedData = true;
                }
            }
            catch (Exception e)
            {
                Log.AddLog(e.Message);
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Чистка завершенных процессов из списка
        /// т.е. удаление всех элементов из ObservableCollection всех элементов с CopyStatus == 2
        /// </summary>
        public static void СlearingСopiedData()
        {
            try
            {
                Log.AddLog("Сlearing copied data");
                canEndCopying = false;
                foreach (var data in notCopiedData.ToList())
                {
                    if (data.CopyStatus == 2)
                    {
                        canUpdateNotCopiedData = false;
                        notCopiedData.Remove(data);
                        canUpdateNotCopiedData = true;
                    }
                }
                id = 0;
                canEndCopying = true;
                Log.AddLog("done");
            }
            catch (Exception e)
            {
                Log.AddLog(e.Message);
                MessageBox.Show(e.Message);
            }
        }

    }
}
