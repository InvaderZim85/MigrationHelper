using System.Windows;
using System.Windows.Controls;
using CmsMigrationHelper.Ui.ViewModel;
using MahApps.Metro.Controls.Dialogs;

namespace CmsMigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for InfoControl.xaml
    /// </summary>
    public partial class InfoControl : UserControl, IUserControl
    {
        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Migration helper info";

        /// <summary>
        /// Creates a new instance of the <see cref="InfoControl"/>
        /// </summary>
        public InfoControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Init the control
        /// </summary>
        public void InitControl()
        {
            if (DataContext is InfoControlViewModel viewModel)
                viewModel.InitViewModel(DialogCoordinator.Instance);
        }

        /// <summary>
        /// Occurs when the control was loaded
        /// </summary>
        private void InfoControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InfoControlViewModel viewModel)
                viewModel.LoadData();
        }
    }
}
