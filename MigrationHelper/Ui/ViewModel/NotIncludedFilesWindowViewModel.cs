using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class NotIncludedFilesWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the instance of the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Backing field for <see cref="NotIncludedFiles"/>
        /// </summary>
        private ObservableCollection<NotIncludedFile> _notIncludedFiles;

        /// <summary>
        /// Gets or sets the list with the not included files
        /// </summary>
        public ObservableCollection<NotIncludedFile> NotIncludedFiles
        {
            get => _notIncludedFiles;
            set => SetField(ref _notIncludedFiles, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedFiles"/>
        /// </summary>
        private List<NotIncludedFile> _selectedFiles = new List<NotIncludedFile>();

        /// <summary>
        /// Gets or sets the selected files
        /// </summary>
        public List<NotIncludedFile> SelectedFiles
        {
            get => _selectedFiles;
            set => SetField(ref _selectedFiles, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The instance of the mah apps dialog coordinator</param>
        /// <param name="notIncludedFiles"></param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator, List<FileInfo> notIncludedFiles)
        {
            _dialogCoordinator = dialogCoordinator;

            NotIncludedFiles =
                new ObservableCollection<NotIncludedFile>(notIncludedFiles.Select(s => (NotIncludedFile) s));
        }

        /// <summary>
        /// The command to include the files
        /// </summary>
        public ICommand IncludeCommand => new DelegateCommand(IncludeFiles);

        /// <summary>
        /// The command to delete the selected files
        /// </summary>
        public ICommand DeleteCommand => new DelegateCommand(DeleteFiles);

        /// <summary>
        /// Includes the selected files into the project
        /// </summary>
        private async void IncludeFiles()
        {
            if (SelectedFiles == null)
                return;

            if (Helper.IncludeProjectFiles(SelectedFiles))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Include",
                    "Selected files successfully added to the project.");

                RemoveFromList(SelectedFiles);
            }
            else
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Include",
                    "An error has occured while including the selected files.");
            }
        }

        /// <summary>
        /// Deletes the selected files
        /// </summary>
        private async void DeleteFiles()
        {
            if (SelectedFiles == null)
                return;

            if (await _dialogCoordinator.ShowMessageAsync(this, "Delete",
                    "Do you really want to delete the selected files?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings{AffirmativeButtonText = "Yes"}) ==
                MessageDialogResult.Negative)
                return;

            if (Helper.DeleteFiles(SelectedFiles))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Delete",
                    "Selected files successfully deleted.");

                RemoveFromList(SelectedFiles);
            }
            else
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Delete",
                    "An error has occured while deleting the selected files.");
            }
        }

        /// <summary>
        /// Removes the entries from the list
        /// </summary>
        /// <param name="files">The list with the entries</param>
        private void RemoveFromList(List<NotIncludedFile> files)
        {
            var tmpList = files.Select(NotIncludedFile.CopyEntry).ToList();

            foreach (var entry in tmpList)
            {
                NotIncludedFiles.Remove(NotIncludedFiles.FirstOrDefault(f => f.FilePath.Equals(entry.FilePath)));
            }
        }
    }
}
