using System;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using ZimLabs.WpfBase;

namespace CmsMigrationHelper.Ui.ViewModel
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
        public ICommand CreateCommand => new DelegateCommand(CreateMigrationFile);

        /// <summary>
        /// The command to load an existing file
        /// </summary>
        public ICommand OpenExistingFileCommand => new DelegateCommand(OpenExistingFile);

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
        }

        /// <summary>
        /// Creates the migration file
        /// </summary>
        private async void CreateMigrationFile()
        {
            try
            {
                var (successful, filename) = Helper.CreateMigrationFile(Filename, _textGetSet.Get());

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
