using System.Windows;
using MahApps.Metro;
using Props = MigrationHelper.Properties;

namespace MigrationHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// Occurs when the application is started
        /// </summary>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ThemeManager.ChangeAppStyle(Current, ThemeManager.GetAccent(Props.Settings.Default.Accent),
                ThemeManager.GetAppTheme(Props.Settings.Default.Theme));
        }

        /// <summary>
        /// Occurs when the application shuts down
        /// </summary>
        private void App_OnExit(object sender, ExitEventArgs e)
        {
            Helper.UnloadProject();
        }
    }
}
