using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class MigrationSettingsControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the instance of the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Backing field for <see cref="ExcludeDirs"/>
        /// </summary>
        private string _excludeDirs;

        /// <summary>
        /// Gets or sets the directories which should be excluded
        /// </summary>
        public string ExcludeDirs
        {
            get => _excludeDirs;
            set => SetField(ref _excludeDirs, value);
        }

        /// <summary>
        /// Backing field for <see cref="UseSubFolder"/>
        /// </summary>
        private bool _useSubFolder;

        /// <summary>
        /// Gets or sets the value which indicates if a sub folder should be used
        /// </summary>
        public bool UseSubFolder
        {
            get => _useSubFolder;
            set
            {
                SetField(ref _useSubFolder, value);
                FormatEnabled = value;
            }
        }

        /// <summary>
        /// Backing field for <see cref="SubFolderFormat"/>
        /// </summary>
        private string _subFolderFormat;

        /// <summary>
        /// Gets or sets the sub folder format
        /// </summary>
        public string SubFolderFormat
        {
            get => _subFolderFormat;
            set => SetField(ref _subFolderFormat, value);
        }

        /// <summary>
        /// Backing field for <see cref="FormatEnabled"/>
        /// </summary>
        private bool _formatEnabled = false;

        /// <summary>
        /// Gets or sets the value which indicates if the format text box is enabled
        /// </summary>
        public bool FormatEnabled
        {
            get => _formatEnabled;
            set => SetField(ref _formatEnabled, value);
        }

        /// <summary>
        /// Backing field for <see cref="FormatList"/>
        /// </summary>
        private ObservableCollection<TextValueItem> _formatList;

        /// <summary>
        /// Gets or sets the list with the format
        /// </summary>
        public ObservableCollection<TextValueItem> FormatList
        {
            get => _formatList;
            set => SetField(ref _formatList, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedFormat"/>
        /// </summary>
        private TextValueItem _selectedFormat;

        /// <summary>
        /// Gets or sets the selected format
        /// </summary>
        public TextValueItem SelectedFormat
        {
            get => _selectedFormat;
            set => SetField(ref _selectedFormat, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

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
            SetFormatList();

            ExcludeDirs = Properties.Settings.Default.ExcludeDirectories;
            UseSubFolder = Properties.Settings.Default.UseSubFolder;
            SubFolderFormat = Properties.Settings.Default.SubScriptDirectory;
            SelectedFormat =
                FormatList.FirstOrDefault(f => f.Id == Properties.Settings.Default.SubScriptDirectoryFormat);
        }

        /// <summary>
        /// Sets the format list
        /// </summary>
        private void SetFormatList()
        {
            FormatList = new ObservableCollection<TextValueItem>(
                (from CustomEnums.SubFolderFormat value in Enum.GetValues(typeof(CustomEnums.SubFolderFormat))
                    select new TextValueItem(value)).ToList());
        }

        /// <summary>
        /// Saves the settings
        /// </summary>
        private async void SaveSettings()
        {
            if (UseSubFolder)
            {
                if (string.IsNullOrEmpty(SubFolderFormat))
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Format", "Please insert a name of the sub folder");
                    return;
                }

                if (SelectedFormat == null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Format", "Please select a format.");
                    return;
                }
            }

            Properties.Settings.Default.ExcludeDirectories = ExcludeDirs;
            Properties.Settings.Default.UseSubFolder = UseSubFolder;
            Properties.Settings.Default.SubScriptDirectory = SubFolderFormat;
            Properties.Settings.Default.SubScriptDirectoryFormat = SelectedFormat?.Id ?? 0;
            Properties.Settings.Default.Save();
        }
    }
}
