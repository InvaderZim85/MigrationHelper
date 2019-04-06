using System;
using System.Windows.Controls;
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
    }
}
