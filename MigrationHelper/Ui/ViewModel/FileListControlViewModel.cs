using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ZimLabs.Utility.Extensions;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class FileListControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the action to set the selected file
        /// </summary>
        private Action<FileInfo> _setSelectedFile;

        /// <summary>
        /// Contains the original list
        /// </summary>
        private List<FileInfo> _originList;

        /// <summary>
        /// Backing field for <see cref="FileList"/>
        /// </summary>
        private ObservableCollection<FileInfo> _fileList;

        /// <summary>
        /// Gets or sets the list with the available files
        /// </summary>
        public ObservableCollection<FileInfo> FileList
        {
            get => _fileList;
            set => SetField(ref _fileList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedFile"/>
        /// </summary>
        private FileInfo _selectedFile;

        /// <summary>
        /// Gets or sets the selected file
        /// </summary>
        public FileInfo SelectedFile
        {
            get => _selectedFile;
            set => SetField(ref _selectedFile, value);
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
        /// Init the view model
        /// </summary>
        /// <param name="setSelectedFile">The action to set the selected file</param>
        public void InitViewModel(Action<FileInfo> setSelectedFile)
        {
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
        /// Filters the list
        /// </summary>
        private void FilterList()
        {
            FileList = new ObservableCollection<FileInfo>(string.IsNullOrEmpty(Filter)
                ? _originList.OrderBy(o => o.CreationTime)
                : _originList.Where(w => w.Name.ContainsIgnoreCase(Filter)).OrderBy(o => o.CreationTime));
        }
    }
}
