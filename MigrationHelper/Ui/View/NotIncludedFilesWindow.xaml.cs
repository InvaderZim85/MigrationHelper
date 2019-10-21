using System.Collections.Generic;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for NotIncludedFilesWindow.xaml
    /// </summary>
    public partial class NotIncludedFilesWindow : MetroWindow
    {
        /// <summary>
        /// Contains the list with the files
        /// </summary>
        private readonly List<FileInfo> _fileList;

        /// <summary>
        /// Creates a new instance of the <see cref="NotIncludedFilesWindow"/>
        /// </summary>
        /// <param name="fileList">The list with the files</param>
        public NotIncludedFilesWindow(List<FileInfo> fileList)
        {
            InitializeComponent();

            _fileList = fileList;
        }

        /// <summary>
        /// Occurs when the form was loaded
        /// </summary>
        private void NotIncludedFilesWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is NotIncludedFilesWindowViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance, _fileList);
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
