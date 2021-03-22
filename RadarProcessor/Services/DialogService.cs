using System;
using System.Windows;
using Microsoft.Win32;

namespace RadarProcessor.Services
{
    public class DialogService : IDialogService
    {
        public string BrowseFile(string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = filter
            };

            return openFileDialog.ShowDialog().GetValueOrDefault() ?
                openFileDialog.FileName :
                string.Empty;
        }

        public void ShowError(string errorMessage)
        {
            MessageBox.Show(
                errorMessage,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public bool ShowMessage(string message)
        {
            throw new NotImplementedException();
        }
    }
}