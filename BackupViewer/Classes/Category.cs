using BackupViewer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupViewer.Classes
{
    class Category
    {
        /// <summary>
        /// Собирает все категории из ObservableCollection<AddDataInfo>
        /// </summary>
        /// <param name="data"></param>
        /// <returns>ObservableCollection<SelectedCategories></returns>
        public ObservableCollection<SelectedCategories> GetCategories(ObservableCollection<AddDataInfo> data)
        {
            ObservableCollection<SelectedCategories>  allСategories = new ObservableCollection<SelectedCategories>();

            List<string> fff = new List<string>();
            foreach (var aaa in data)
            {
                foreach (var bbb in aaa.Category)
                {
                    fff.Add(bbb.Value);
                }
            }
            if (fff != null)
            {
                Log.AddLog("Categories found:");

                fff = fff.Distinct().ToList();

                foreach (var aaa in fff)
                {
                    allСategories.Add(new SelectedCategories
                    {
                        Name = aaa,
                        SomeItemSelected = false
                    });
                    Log.AddLog('\t' + aaa);
                }
                Log.AddLog("done.");
            }

            return allСategories;
        }

        /// <summary>
        /// Собирает все категории, удаляет пустые строки и удаляет дубликаты
        /// </summary>
        /// <param name="newCategories">Новые категории</param>
        /// <param name="currentCategories">Список старых категорий</param>
        /// <returns>ObservableCollection<CategoryItem></returns>
        public ObservableCollection<CategoryItem> RemoveDuplicates(ObservableCollection<CategoryItem> newCategories, ObservableCollection<SelectedCategories> currentCategories)
        {
            Log.AddLog('\t' + "Combining categories.");
            foreach (var item in currentCategories)
            {
                if (item.SomeItemSelected)
                    newCategories.Add(new CategoryItem { Value = item.Name });
            }

            for (int i = 0; i < newCategories.Count; i++)
            {
                if (newCategories[i].Value == "")
                {
                    newCategories.RemoveAt(i);
                    i--;
                }
            }

            Log.AddLog('\t' + "Deleting duplicate categories.");
            for (int i = 0; i < newCategories.Count; i++)
            {
                for (int k = 0; k < newCategories.Count; k++)
                {
                    if (k != i && newCategories[i].Value.ToLower() == newCategories[k].Value.ToLower())
                    {
                        newCategories.RemoveAt(k);
                        k--;
                    }
                }
            }

            return newCategories;
        }

        /// <summary>
        /// Собирает все категории, удаляет пустые строки и удаляет дубликаты
        /// </summary>
        /// <param name="newCategories">Новые категории</param>
        /// <param name="currentCategories">Список старых категорий</param>
        /// <returns>ObservableCollection<CategoryItem></returns>
        public ObservableCollection<SelectedCategories> AddNewCategories(ObservableCollection<CategoryItem> newCategories, ObservableCollection<SelectedCategories> currentCategories)
        {
            Log.AddLog('\t' + "Combining categories.");
            foreach (var item in newCategories)
            {
                currentCategories.Add(new SelectedCategories { Name = item.Value, SomeItemSelected = false });
            }

            for (int i = 0; i < newCategories.Count; i++)
            {
                if (newCategories[i].Value == "")
                {
                    newCategories.RemoveAt(i);
                    i--;
                }
            }

            Log.AddLog('\t' + "Deleting duplicate categories.");
            for (int i = 0; i < currentCategories.Count; i++)
            {
                for (int k = 0; k < currentCategories.Count; k++)
                {
                    if (k != i && currentCategories[i].Name.ToLower() == currentCategories[k].Name.ToLower())
                    {
                        currentCategories.RemoveAt(k);
                        k--;
                    }
                }
            }

            return currentCategories;
        }
    }
}
