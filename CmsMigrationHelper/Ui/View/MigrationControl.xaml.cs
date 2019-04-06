using System.Windows.Controls;

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
        }
    }
}
