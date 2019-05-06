using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using ZimLabs.Utility.Extensions;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class FileListControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Contains the action to set the selected file
        /// </summary>
        private Action<FileItem> _setSelectedFile;

        /// <summary>
        /// Contains the original list
        /// </summary>
        private List<FileItem> _originList;

        /// <summary>
        /// Backing field for <see cref="FileList"/>
        /// </summary>
        private ObservableCollection<FileItem> _fileList;

        /// <summary>
        /// Gets or sets the list with the available files
        /// </summary>
        public ObservableCollection<FileItem> FileList
        {
            get => _fileList;
            set => SetField(ref _fileList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedFile"/>
        /// </summary>
        private FileItem _selectedFile;

        /// <summary>
        /// Gets or sets the selected file
        /// </summary>
        public FileItem SelectedFile
        {
            get => _selectedFile;
            set
            {
                SetField(ref _selectedFile, value);
                DeleteButtonEnabled = value != null;
            }
        }

        /// <summary>
        /// Backing field for <see cref="Filter"/>
        /// </summary>
        private string _filter;

        /// <summary>
        /// Gets or sets the filter
        /// </summary>
        public string Filter
        {
            get => _filter;
            set => SetField(ref _filter, value);
        }

        /// <summary>
        /// Backing field for <see cref="DeleteButtonEnabled"/>
        /// </summary>
        private bool _deleteButtonEnabled;

        /// <summary>
        /// Gets or sets the value which indicates if the delete button is enabled
        /// </summary>
        public bool DeleteButtonEnabled
        {
            get => _deleteButtonEnabled;
            set => SetField(ref _deleteButtonEnabled, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        /// <param name="setSelectedFile">The action to set the selected file</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator, Action<FileItem> setSelectedFile)
        {
            _dialogCoordinator = dialogCoordinator;

            _setSelectedFile = setSelectedFile;

            LoadFiles();
        }

        /// <summary>
        /// Loads the files
        /// </summary>
        public void LoadFiles()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                return;

            _originList = Helper.LoadScriptFiles();

            FilterList();
        }

        /// <summary>
        /// The command to filter the file list
        /// </summary>
        public ICommand FilterCommand => new DelegateCommand(FilterList);

        /// <summary>
        /// The command to open the selected file
        /// </summary>
        public ICommand OpenCommand => new DelegateCommand(() =>
        {
            if (SelectedFile != null)
                _setSelectedFile(SelectedFile);
        });

        /// <summary>
        /// The command to delete the selected entry
        /// </summary>
        public ICommand DeleteCommand => new DelegateCommand(DeleteEntry);

        /// <summary>
        /// Filters the list
        /// </summary>
        private void FilterList()
        {
            FileList = new ObservableCollection<FileItem>(string.IsNullOrEmpty(Filter)
                ? _originList.OrderBy(o => o.File.CreationTime)
                : _originList.Where(w => w.Name.ContainsIgnoreCase(Filter)).OrderBy(o => o.File.CreationTime));
        }

        /// <summary>
        /// Deletes the selected entry
        /// </summary>
        private async void DeleteEntry()
        {
            if (SelectedFile == null)
                return;

            if (await _dialogCoordinator.ShowMessageAsync(this, "Delete",
                    $"Do you really want to delete the migration file \"{SelectedFile.Name}\"") ==
                MessageDialogResult.Negative)
                return;

            try
            {
                Helper.DeleteProjectItem(SelectedFile);

                SelectedFile = null;

                LoadFiles();

                _setSelectedFile(null);
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(DeleteEntry), ex);
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occured.\r\n\r\nMessage: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Sets the selected file
        /// </summary>
        /// <param name="filename">The name of the file</param>
        public FileItem SetSelectedFile(string filename)
        {
            var item = FileList.FirstOrDefault(f => f.Name.EqualsIgnoreCase(filename));

            SelectedFile = item;
            return item;
        }
    }
}
