using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.Model
{
    public class Directory : BaseVM
    {
        /// <summary>
        /// Полный путь
        /// </summary>
        public string Path_ { get; set; }
        /// <summary>
        /// Расширение
        /// null - папка
        /// </summary>
        public string Extension { get; set; }

        //public Directory(string value)
        //{
        //    Path_ = value;
        //}
    }
}
