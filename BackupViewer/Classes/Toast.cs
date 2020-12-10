using BackupViewer.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BackupViewer.Classes
{
    static class Toast
    {
        private static readonly ToastViewModel _toast = new ToastViewModel();
        public static void ToastInitialization()
        {
            Application.Current.MainWindow.Closing += MainWindowOnClosing;
        }
        private static void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _toast.OnUnloaded();
        }

        public static void ShowInformation(string text)
        {
            _toast.ShowInformation(text, null);
        }
        public static void ShowSuccess(string text)
        {
            _toast.ShowSuccess(text, null);
        }
        public static void ShowError(string text)
        {
            _toast.ShowError(text, null);
        }
        public static void ShowWarning(string text)
        {
            _toast.ShowWarning(text, null);
        }
    }
}
