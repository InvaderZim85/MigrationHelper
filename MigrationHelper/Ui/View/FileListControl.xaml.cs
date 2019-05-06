using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for FileListControl.xaml
    /// </summary>
    public partial class FileListControl : UserControl
    {
        /// <summary>
        /// The delegate for the <see cref="SelectionChanged"/> event
        /// </summary>
        /// <param name="file">The selected file</param>
        public delegate void SelectionChangedEventHandler(FileItem file);

        /// <summary>
        /// Occurs when the user selects another file
        /// </summary>
        [Browsable(true), Description("Occurs when the user selects another file entry")]
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Creates a new instance of the <see cref="FileListControl"/>
        /// </summary>
        public FileListControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is FileListControlViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance, SelectionChangedNotifier);
        }

        /// <summary>
        /// Reloads the file list
        /// </summary>
        public void Reload()
        {
            if (DataContext is FileListControlViewModel viewModel)
                viewModel.LoadFiles();
        }

        /// <summary>
        /// Sets the selected file
        /// </summary>
        /// <param name="filename">The name of the file</param>
        public void SetSelectedFile(string filename)
        {
            if (!(DataContext is FileListControlViewModel viewModel))
                return;

            var item = viewModel.SetSelectedFile(filename);
            DataGrid.ScrollIntoView(item);
        }

        /// <summary>
        /// Fires the <see cref="SelectionChanged"/> event when the user selects another file
        /// </summary>
        /// <param name="file">The selected file</param>
        private void SelectionChangedNotifier(FileItem file)
        {
            SelectionChanged?.Invoke(file);
        }
    }
}
