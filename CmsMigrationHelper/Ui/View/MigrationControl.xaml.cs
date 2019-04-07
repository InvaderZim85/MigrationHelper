using System.Windows.Controls;
using CmsMigrationHelper.DataObjects;
using CmsMigrationHelper.Ui.ViewModel;
using MahApps.Metro.Controls.Dialogs;

namespace CmsMigrationHelper.Ui.View
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
                viewModel.InitViewModel(DialogCoordinator.Instance, (SetSqlText, GetSqlText));

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
    }
}
