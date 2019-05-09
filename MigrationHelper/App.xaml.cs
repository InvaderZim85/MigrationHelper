using System.Windows;

namespace MigrationHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Occurs when the application shuts down
        /// </summary>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Helper.UnloadProject();
        }
    }
}
