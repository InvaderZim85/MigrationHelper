using System.IO;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for MigrationControl.xaml
    /// </summary>
    public partial class MigrationControl : UserControl, IUserControl
    {
        /// <summary>
        /// Gets the description of the control
        /// </summary>
        public string Description => "Migration helper";

        /// <summary>
        /// Creates a new instance of the <see cref="MigrationControl"/>
        /// </summary>
        public MigrationControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            SqlEditor.Options.HighlightCurrentLine = true;
            SqlEditor.SyntaxHighlighting = Helper.LoadSqlSchema();
            if (DataContext is MigrationControlViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance, (SetSqlText, GetSqlText), UpdateFileList, SetSelectedFile);

            FileList.InitControl();
        }

        /// <summary>
        /// Updates the file list
        /// </summary>
        private void UpdateFileList()
        {
            FileList.Reload();
        }

        /// <summary>
        /// Gets the text of the avalon editor
        /// </summary>
        /// <returns>The text</returns>
        private string GetSqlText()
        {
            return SqlEditor.Text;
        }

        /// <summary>
        /// Sets the text of the avalon editor
        /// </summary>
        /// <param name="text">The desired text</param>
        private void SetSqlText(string text)
        {
            SqlEditor.Text = text;
        }

        /// <summary>
        /// Sets the selected file
        /// </summary>
        /// <param name="filename">The name of the file</param>
        private void SetSelectedFile(string filename)
        {
            FileList.SetSelectedFile(filename);
        }

        /// <summary>
        /// Occurs when the user performs a double click on the error entry
        /// </summary>
        /// <param name="entry">The selected entry</param>
        private void SqlErrorControl_OnDoubleClick(ErrorEntry entry)
        {
            SqlEditor.Focus();
            SqlEditor.ScrollTo(entry.Line, entry.Column);
            SqlEditor.TextArea.Caret.Line = entry.Line;
            SqlEditor.TextArea.Caret.Column = entry.Column;
        }

        /// <summary>
        /// Occurs when the user selects a file in the file list
        /// </summary>
        /// <param name="file">The selected file</param>
        private void FileList_OnSelectionChanged(FileItem file)
        {
            if (DataContext is MigrationControlViewModel viewModel)
            {
                if (file == null)
                    viewModel.ClearInput();
                else
                    viewModel.OpenSelectedFile(file);
            }
        }
    }
}
