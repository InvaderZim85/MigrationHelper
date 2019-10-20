using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class NotIncludedFilesWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Backing field for <see cref="NotIncludedFiles"/>
        /// </summary>
        private ObservableCollection<FileInfo> _notIncludedFiles;

        /// <summary>
        /// Gets or sets the list with the not included files
        /// </summary>
        public ObservableCollection<FileInfo> NotIncludedFiles
        {
            get => _notIncludedFiles;
            set => SetField(ref _notIncludedFiles, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="notIncludedFiles"></param>
        public void InitViewModel(List<FileInfo> notIncludedFiles)
        {
            NotIncludedFiles = new ObservableCollection<FileInfo>(notIncludedFiles);
        }
    }
}
