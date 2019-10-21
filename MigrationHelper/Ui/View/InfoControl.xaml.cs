using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
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

        /// <summary>
        /// Occurs when the user hits the link
        /// </summary>
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }
    }
}
