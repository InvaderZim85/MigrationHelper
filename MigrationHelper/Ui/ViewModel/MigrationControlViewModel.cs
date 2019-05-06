﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs;
using MigrationHelper.DataObjects;
using NLog.Fluent;
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
        /// Contains the action to update the file list
        /// </summary>
        private Action _updateList;

        /// <summary>
        /// Contains the action to set the selected file in the file list
        /// </summary>
        private Action<string> _setSelectedFile;

        /// <summary>
        /// Backing field for <see cref="ProjectFile"/>
        /// </summary>
        private string _projectFile;

        /// <summary>
        /// Contains the value which indicates if an existing file was opened
        /// </summary>
        private bool _existingFile;

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

                _updateList();
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

        /// <summary>
        /// Backing field for <see cref="ShowErrorIcon"/>
        /// </summary>
        private Visibility _showErrorIcon = Visibility.Hidden;

        /// <summary>
        /// Gets or sets the visibility of the error icon
        /// </summary>
        public Visibility ShowErrorIcon
        {
            get => _showErrorIcon;
            set => SetField(ref _showErrorIcon, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowValidIcon"/>
        /// </summary>
        private Visibility _showValidIcon = Visibility.Hidden;

        /// <summary>
        /// Gets or sets the visibility of the valid icon
        /// </summary>
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
        /// <param name="textGetSet">The function to get / set the sql text</param>
        /// <param name="updateList">The action to update the file list</param>
        /// <param name="setSelectedFile">The action to set the selected file in the file list</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator,
            (Action<string> setText, Func<string> getText) textGetSet, Action updateList, Action<string> setSelectedFile)
        {
            _dialogCoordinator = dialogCoordinator;
            _textGetSet = textGetSet;
            _updateList = updateList;
            _setSelectedFile = setSelectedFile;

            ProjectFile = Properties.Settings.Default.ProjectFile;
            ScriptDir = Properties.Settings.Default.ScriptDirectory;
        }

        /// <summary>
        /// The command to open the desired project file
        /// </summary>
        public ICommand OpenProjectFileCommand => new DelegateCommand(OpenProjectFile);

        /// <summary>
        /// The command to load an existing file
        /// </summary>
        public ICommand OpenExistingFileCommand => new DelegateCommand(OpenExistingFile);

        /// <summary>
        /// The command to check the sql script
        /// </summary>
        public ICommand CheckCommand => new DelegateCommand(CheckScript);

        /// <summary>
        /// The command to save the script
        /// </summary>
        public ICommand SaveCommand => new DelegateCommand(SaveScript);

        /// <summary>
        /// The command to create a new migration script
        /// </summary>
        public ICommand NewCommand => new DelegateCommand(ClearInput);

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

                Helper.UpdateProject();

                _updateList();
            }
        }

        /// <summary>
        /// Clears the input for a new entry
        /// </summary>
        public void ClearInput()
        {
            Filename = "";
            _textGetSet.Set("");
            ShowErrorControl = false;
            ErrorList = new ObservableCollection<ErrorEntry>();

            ShowErrorIcon = Visibility.Hidden;
            ShowValidIcon = Visibility.Hidden;
        }

        /// <summary>
        /// Checks the sql script
        /// </summary>
        private async Task<bool> CheckSqlScript()
        {
            var sql = _textGetSet.Get();

            if (string.IsNullOrEmpty(sql))
                return false;

            var controller = await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                "Please wait while checking the script...");
            controller.SetIndeterminate();

            try
            {
                var result = await Helper.IsSqlValid(sql);
                if (result.valid)
                {
                    ShowErrorControl = false;
                    ShowErrorIcon = Visibility.Hidden;
                    ShowValidIcon = Visibility.Visible;
                    ErrorList = new ObservableCollection<ErrorEntry>();
                    return true;
                }
                else
                {
                    ShowErrorControl = true;
                    ShowErrorIcon = Visibility.Visible;
                    ShowValidIcon = Visibility.Hidden;
                    ErrorList = new ObservableCollection<ErrorEntry>(result.errors);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(CheckSqlScript), ex);
                await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured: {ex.Message}");
                return false;
            }
            finally
            {
                await controller.CloseAsync();
            }
        }

        /// <summary>
        /// Starts the sql script check
        /// </summary>
        private async void CheckScript()
        {
            await CheckSqlScript();
        }

        /// <summary>
        /// Saves the script
        /// </summary>
        private async void SaveScript()
        {
            var sql = _textGetSet.Get();

            if (string.IsNullOrEmpty(sql))
                return;

            try
            {
                var sqlValid = await CheckSqlScript();
                var saveScript = true;

                if (!sqlValid)
                {
                    saveScript = await _dialogCoordinator.ShowMessageAsync(this, "SQL",
                                     "The sql script is not valid. Do you really want to save it?",
                                     MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative;

                    if (saveScript)
                        Logger.Info("The migration script contains errors but will be saved.");
                }

                if (saveScript)
                    SaveMigrationFile(sql);
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(SaveScript), ex);
                await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the migration file
        /// </summary>
        /// <param name="sql">The sql script</param>
        private async void SaveMigrationFile(string sql)
        {
            var controller = await _dialogCoordinator.ShowProgressAsync(this, "Please wait",
                "Please wait while creating the migration script...");

            try
            {
                var (successful, filename) = await Task.Run(() => Helper.SaveMigrationFile(Filename, sql, _existingFile));

                if (successful)
                {
                    Filename = filename;
                    
                    _existingFile = true;
                    _updateList();
                    _setSelectedFile(filename);

                    await _dialogCoordinator.ShowMessageAsync(this, "File",
                        $"Migration file created / updated. Filename: {filename}");
                    Logger.Info($"Migration file created / updated. Filename: {filename}");
                }
                else
                    await _dialogCoordinator.ShowMessageAsync(this, "Error",
                        "An error has occured while creating the migration file.");
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(SaveMigrationFile), ex);
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
                SetSqlText(dialog.FileName);
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(OpenExistingFile), ex);
                await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured while loading the content of the selected file.\r\n\r\nMessage: {ex.Message}"); 
            }
        }

        /// <summary>
        /// Opens an existing file
        /// </summary>
        /// <param name="file">The file</param>
        public void OpenSelectedFile(FileItem file)
        {
            _existingFile = true;

            Filename = file.Name.Replace(file.File.Extension, "");

            SetSqlText(file.File.FullName);
        }

        /// <summary>
        /// Loads the text of a given file and shows it
        /// </summary>
        /// <param name="filepath">The path of the file</param>
        private void SetSqlText(string filepath)
        {
            _textGetSet.Set(File.ReadAllText(filepath));
        }
    }
}
