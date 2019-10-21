using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for MigrationSettingsControl.xaml
    /// </summary>
    public partial class MigrationSettingsControl : IUserControl
    {
        /// <summary>
        /// Gets the description of the control
        /// </summary>
        public string Description { get; } = "Migration settings";

        /// <summary>
        /// Creates a new instance of the control
        /// </summary>
        public MigrationSettingsControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Init the user control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is MigrationSettingsControlViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance);
        }
    }
}
