using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using MigrationHelper.Ui.ViewModel;
using ZimLabs.Utility.Extensions;

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
            SetSqlSchema();

            Mediator.Register("SetSqlSchema", SetSqlSchema);

            // A load is here not needed, because it's triggered by the ScriptDir property which is set in the init method
            // of the MigrationControlViewModel
            FileList.InitControl();

            if (DataContext is MigrationControlViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance, (SetSqlText, GetSqlText), UpdateFileList, SetSelectedFile);
        }

        /// <summary>
        /// Sets the sql schema of the editor window
        /// </summary>
        private void SetSqlSchema()
        {
            var dark = Properties.Settings.Default.Theme.ContainsIgnoreCase("dark");

            SqlEditor.Options.HighlightCurrentLine = true;
            SqlEditor.SyntaxHighlighting = Helper.LoadSqlSchema(dark);
            SqlEditor.Foreground = new SolidColorBrush(dark ? Colors.White : Colors.Black);
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
        /// Occurs when the user performs a double click on the error entry.
        /// </summary>
        /// <param name="entry">The selected entry</param>
        private void SqlErrorControl_OnDoubleClick(ErrorEntry entry)
        {
            // INFO: Do not remove this class. ReSharper doesn't get it, that it's used in the ui
            SqlEditor.Focus();
            SqlEditor.ScrollTo(entry.Line, entry.Column);
            SqlEditor.TextArea.Caret.Line = entry.Line;
            SqlEditor.TextArea.Caret.Column = entry.Column;
        }

        /// <summary>
        /// Occurs when the user selects a file in the file list
        /// </summary>
        /// <param name="file">The selected file</param>
        private void FileList_OnSelectionChanged(TreeViewNode file)
        {
            // INFO: Do not remove this class. ReSharper doesn't get it, that it's used in the ui
            if (DataContext is MigrationControlViewModel viewModel)
            {
                if (file == null)
                    viewModel.ClearInput();
                else
                {
                    viewModel.OpenSelectedFile(file);
                    viewModel.HasChanges = false;
                }
            }
        }

        /// <summary>
        /// Occurs when the user edits the text
        /// </summary>
        private void SqlEditor_OnTextChanged(object sender, EventArgs e)
        {
            if (DataContext is MigrationControlViewModel viewModel && !viewModel.HasChanges)
                viewModel.HasChanges = true;
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void MigrationControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MigrationControlViewModel viewModel)
                viewModel.LoadData();
        }
    }
}
