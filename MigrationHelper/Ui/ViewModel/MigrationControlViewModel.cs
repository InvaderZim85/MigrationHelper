using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using MigrationHelper.DataObjects;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class MigrationControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the instance of the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Contains the text getter / setter for the avalon edit control because it's not bind able
        /// </summary>
        private (Action<string> Set, Func<string> Get) _textGetSet;

        /// <summary>
        /// Backing field for <see cref="ProjectFile"/>
        /// </summary>
        private string _projectFile;

        /// <summary>
        /// Gets or sets the path of the project file
        /// </summary>
        public string ProjectFile
        {
            get => _projectFile;
            set => SetField(ref _projectFile, value);
        }

        /// <summary>
        /// Backing field for <see cref="ScriptDir"/>
        /// </summary>
        private string _scriptDir;

        /// <summary>
        /// Gets or sets the name of the script directory
        /// </summary>
        public string ScriptDir
        {
            get => _scriptDir;
            set
            {
                SetField(ref _scriptDir, value);
                Properties.Settings.Default.ScriptDirectory = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Backing field for <see cref="Filename"/>
        /// </summary>
        private string _filename;

        /// <summary>
        /// Contains the desired file name
        /// </summary>
        public string Filename
        {
            get => _filename;
            set
            {
                SetField(ref _filename, value);
                CreateButtonEnabled = !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(ProjectFile) &&
                                      File.Exists(ProjectFile);
            }
        }

        /// <summary>
        /// Backing field for <see cref="CreateButtonEnabled"/>
        /// </summary>
        private bool _createButtonEnabled;

        /// <summary>
        /// Gets or sets the value which indicates if the create button is enabled
        /// </summary>
        public bool CreateButtonEnabled
        {
            get => _createButtonEnabled;
            set => SetField(ref _createButtonEnabled, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowErrorControl"/>
        /// </summary>
        private bool _showErrorControl = false;

        /// <summary>
        /// Gets or sets the value which indicates if the error control should be shown
        /// </summary>
        public bool ShowErrorControl
        {
            get => _showErrorControl;
            set => SetField(ref _showErrorControl, value);
        }

        private Visibility _showErrorIcon = Visibility.Hidden;

        public Visibility ShowErrorIcon
        {
            get => _showErrorIcon;
            set => SetField(ref _showErrorIcon, value);
        }

        private Visibility _showValidIcon = Visibility.Hidden;

        public Visibility ShowValidIcon
        {
            get => _showValidIcon;
            set => SetField(ref _showValidIcon, value);
        }


        /// <summary>
        /// Backing field for <see cref="ErrorList"/>
        /// </summary>
        private ObservableCollection<ErrorEntry> _errorList;

        /// <summary>
        /// Gets or sets the list with the errors
        /// </summary>
        public ObservableCollection<ErrorEntry> ErrorList
        {
            get => _errorList;
            set => SetField(ref _errorList, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        /// <param name="textGetSet"></param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator,
            (Action<string> setText, Func<string> getText) textGetSet)
        {
            _dialogCoordinator = dialogCoordinator;
            _textGetSet = textGetSet;

            ProjectFile = Properties.Settings.Default.ProjectFile;
            ScriptDir = Properties.Settings.Default.ScriptDirectory;
        }

        /// <summary>
        /// The command to open the desired project file
        /// </summary>
        public ICommand OpenProjectFileCommand => new DelegateCommand(OpenProjectFile);

        /// <summary>
        /// The command to clear the input
        /// </summary>
        public ICommand ClearCommand => new DelegateCommand(Clear);

        /// <summary>
        /// The command to create the migration file
        /// </summary>
        public ICommand CreateWithCheckCommand => new DelegateCommand(() => CreateScript(true));

        /// <summary>
        /// The command to load an existing file
        /// </summary>
        public ICommand OpenExistingFileCommand => new DelegateCommand(OpenExistingFile);

        /// <summary>
        /// The command to create the migration file without a check
        /// </summary>
        public ICommand CreateWithoutCheckCommand => new DelegateCommand(() => CreateScript(false));

        /// <summary>
        /// The command to check the sql script
        /// </summary>
        public ICommand CheckCommand => new DelegateCommand(CheckSqlScript);

        /// <summary>
        /// Opens the project file
        /// </summary>
        private void OpenProjectFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Title = "Select the project file of the migration project",
                Filters = {new CommonFileDialogFilter("Project file", ".csproj")},
                Multiselect = false
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ProjectFile = dialog.FileName;
                Properties.Settings.Default.ProjectFile = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Clears the input
        /// </summary>
        private void Clear()
        {
            Filename = "";
            _textGetSet.Set("");
            ShowErrorControl = false;
            ErrorList = new ObservableCollection<ErrorEntry>();

            ShowErrorIcon = Visibility.Hidden;
            ShowValidIcon = Visibility.Hidden;
        }

        /// <summary>
        /// Checks the inserted sql script
        /// </summary>
        /// <returns>true when valid, otherwise false</returns>
        private async Task<bool> CheckSql()
        {
            var sql = _textGetSet.Get();

            if (string.IsNullOrEmpty(sql))
                return true;

            var controller = await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                "Please wait while checking the script...");
            controller.SetIndeterminate();

            try
            {
                var result = await Helper.IsSqlValid(sql);
                if (!result.valid)
                {
                    ShowErrorControl = true;
                    ShowErrorIcon = Visibility.Visible;
                    ShowValidIcon = Visibility.Hidden;
                    ErrorList = new ObservableCollection<ErrorEntry>(result.errors);
                    return false;
                }
                else
                {
                    ShowErrorControl = false;
                    ShowErrorIcon = Visibility.Hidden;
                    ShowValidIcon = Visibility.Visible;
                    ErrorList = new ObservableCollection<ErrorEntry>();
                    return true;
                }
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured: {ex.Message}");
                return false;
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Checks the sql script
        /// </summary>
        private async void CheckSqlScript()
        {
            await CheckSql();
        }

        /// <summary>
        /// Creates the migration script 
        /// </summary>
        /// <param name="withCheck">true to check the sql script, otherwise false</param>
        private async void CreateScript(bool withCheck)
        {
            var sql = _textGetSet.Get();

            if (withCheck)
            {
                var result = await CheckSql();
                if (!result)
                    return;
            }
            else
            {
                if (await _dialogCoordinator.ShowMessageAsync(this, "Create script",
                        "Do you really want to create the script without check?",
                        MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Negative)
                    return;
            }

            CreateMigrationFile(sql);
        }

        /// <summary>
        /// Creates the migration file
        /// </summary>
        /// <param name="sql">The sql script</param>
        private async void CreateMigrationFile(string sql)
        {
            var controller = await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                "Please wait while creating the migration script...");

            try
            {
                var (successful, filename) = await Task.Run(() => Helper.CreateMigrationFile(Filename, sql));

                if (successful)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "File",
                        $"Migration file created. Filename: {filename}");
                    Clear();
                }
                else
                    await _dialogCoordinator.ShowMessageAsync(this, "Error",
                        "An error has occured while creating the migration file.");
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occured.\r\n\r\nMessage: {ex.Message}");
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Opens an existing file, extracts the content and inserts it into the sql editor
        /// </summary>
        private async void OpenExistingFile()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = false,
                Title = "Select desired file",
                Filters =
                {
                    new CommonFileDialogFilter("SQL file", ".sql"), new CommonFileDialogFilter("Any file", "*.*")
                },
                Multiselect = false
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            try
            {
                var content = File.ReadAllText(dialog.FileName);

                _textGetSet.Set(content);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured while loading the content of the selected file.\r\n\r\nMessage: {ex.Message}"); 
            }
        }
    }
}
