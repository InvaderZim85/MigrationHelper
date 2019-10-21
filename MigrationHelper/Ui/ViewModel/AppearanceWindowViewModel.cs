using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class AppearanceWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the value which indicates if the control was initialized
        /// </summary>
        private bool _init;

        /// <summary>
        /// The action to close the window
        /// </summary>
        private Action _closeAction;

        /// <summary>
        /// Backing field for <see cref="ThemeList"/>
        /// </summary>
        private ObservableCollection<string> _themeList = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the theme list
        /// </summary>
        public ObservableCollection<string> ThemeList
        {
            get => _themeList;
            set => SetField(ref _themeList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedTheme"/>
        /// </summary>
        private string _selectedTheme;

        /// <summary>
        /// Gets or sets the selected theme
        /// </summary>
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetField(ref _selectedTheme, value))
                    ChangeTheme();
            }
        }

        /// <summary>
        /// Backing field for <see cref="AccentList"/>
        /// </summary>
        private ObservableCollection<string> _accentList = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets the accent list
        /// </summary>
        public ObservableCollection<string> AccentList
        {
            get => _accentList;
            set => SetField(ref _accentList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedAccent"/>
        /// </summary>
        private string _selectedAccent;

        /// <summary>
        /// Gets or sets the selected accent
        /// </summary>
        public string SelectedAccent
        {
            get => _selectedAccent;
            set
            {
                if (SetField(ref _selectedAccent, value))
                    ChangeTheme();
            }
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="closeAction">The action to close the window</param>
        public void InitViewModel(Action closeAction)
        {
            _closeAction = closeAction;
            LoadSettings();
        }

        /// <summary>
        /// The command to save the settings
        /// </summary>
        public ICommand SaveCommand => new DelegateCommand(SaveSettings);

        /// <summary>
        /// Loads the settings
        /// </summary>
        private void LoadSettings()
        {
            _init = true;

            ThemeList = new ObservableCollection<string>(ThemeManager.AppThemes.Select(s => s.Name));
            AccentList = new ObservableCollection<string>(ThemeManager.Accents.Select(s => s.Name));

            SelectedTheme = Properties.Settings.Default.Theme;
            SelectedAccent = Properties.Settings.Default.Accent;

            _init = false;
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        private void SaveSettings()
        {
            if (!Properties.Settings.Default.Accent.Equals(SelectedAccent))
                Properties.Settings.Default.Accent = SelectedAccent;

            if (!Properties.Settings.Default.Theme.Equals(SelectedTheme))
                Properties.Settings.Default.Theme = SelectedTheme;

            Mediator.Execute("SetSqlSchema");

            Properties.Settings.Default.Save();

            _closeAction();
        }

        /// <summary>
        /// Changes the current theme
        /// </summary>
        public void ChangeTheme(bool toDefault = false)
        {
            if (_init)
                return;

            var theme = Properties.Settings.Default.Theme;
            var accent = Properties.Settings.Default.Accent;

            if (!toDefault)
            {
                if (!string.IsNullOrEmpty(SelectedAccent))
                    accent = SelectedAccent;

                if (!string.IsNullOrEmpty(SelectedTheme))
                    theme = SelectedTheme;
            }

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(accent),
                ThemeManager.GetAppTheme(theme));
        }
    }
}
