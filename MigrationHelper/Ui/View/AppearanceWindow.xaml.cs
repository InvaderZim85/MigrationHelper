using System.Windows;
using MahApps.Metro.Controls;
using MigrationHelper.Ui.ViewModel;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for AppearanceWindow.xaml
    /// </summary>
    public partial class AppearanceWindow : MetroWindow
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AppearanceWindow"/>
        /// </summary>
        public AppearanceWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        private void CloseWindow()
        {
            Close();
        }

        /// <summary>
        /// Occurs when the window was loaded
        /// </summary>
        private void AppearanceWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AppearanceWindowViewModel viewModel)
                viewModel.InitViewModel(CloseWindow);
        }

        /// <summary>
        /// Occurs when the user hits the close button
        /// </summary>
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            // Set the current theme
            if (DataContext is AppearanceWindowViewModel viewModel)
                viewModel.ChangeTheme(true);

            Close();
        }
    }
}
