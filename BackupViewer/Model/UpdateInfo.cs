using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.Model
{
    class UpdateInfo
    {
        /// <summary>
        /// Версия
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Описание обновления
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Прямая ссылка на архив
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// Требуется ли обновение
        /// </summary>
        public bool NeedToUpdate { get; set; }
    }
}
