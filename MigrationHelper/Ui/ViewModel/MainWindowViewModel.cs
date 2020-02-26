using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.Ui.View;
using ZimLabs.Utility;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Contains the dictionary with the controls
        /// </summary>
        private readonly Dictionary<MenuItemType, IUserControl> _controlDictionary = new Dictionary<MenuItemType, IUserControl>();

        /// <summary>
        /// Backing field for <see cref="Control"/>
        /// </summary>
        private object _control;
        
        /// <summary>
        /// Gets or sets the selected control
        /// </summary>
        public object Control
        {
            get => _control;
            set => SetField(ref _control, value);
        }

        /// <summary>
        /// Backing field for <see cref="ControlDescription"/>
        /// </summary>
        private string _controlDescription = "Migration helper";

        /// <summary>
        /// Gets or sets the description of the control
        /// </summary>
        public string ControlDescription
        {
            get => _controlDescription;
            set => SetField(ref _controlDescription, value);
        }

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string _title = "Migration Helper";

        /// <summary>
        /// Gets or sets the title of the main window
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        /// <summary>
        /// Backing field for <see cref="BranchName"/>
        /// </summary>
        private string _branchName;

        /// <summary>
        /// Gets or sets the name of the currently selected branch
        /// </summary>
        public string BranchName
        {
            get => _branchName;
            set => SetField(ref _branchName, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            Mediator.Register(nameof(SetBranchName), SetBranchName);

            var (_, _, _, productVersion) = Helper.GetVersion();

            Title = $"Migration Helper (v{productVersion})";

            SwitchControl(MenuItemType.Migration);
        }

        /// <summary>
        /// Checks the properties at the start up
        /// </summary>
        public async void CheckProperties()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile) ||
                File.Exists(Properties.Settings.Default.ProjectFile)) 
                return;

            // Remove the project file
            Properties.Settings.Default.ProjectFile = "";
            Properties.Settings.Default.Save();

            await _dialogCoordinator.ShowMessageAsync(this, "Error", "The project file cannot be found. Reset settings.");
        }

        /// <summary>
        /// The menu command to select another control
        /// </summary>
        public ICommand MenuCommand => new RelayCommand<MenuItemType>(SwitchControl);

        /// <summary>
        /// Switches between the controls
        /// </summary>
        /// <param name="type">The desired type</param>
        private async void SwitchControl(MenuItemType type)
        {
            IUserControl control = null;

            if (type == MenuItemType.Appearance)
            {
                var appearanceWindow = new AppearanceWindow();
                appearanceWindow.ShowDialog();
                return;
            }

            if (_controlDictionary.ContainsKey(type))
            {
                control = _controlDictionary[type];
            }
            else
            {
                switch (type)
                {
                    case MenuItemType.Migration:
                        control = new MigrationControl();
                        break;
                    case MenuItemType.MigrationSettings:
                        control = new MigrationSettingsControl();
                        break;
                    case MenuItemType.Info:
                        control = new InfoControl();
                        break;
                    case MenuItemType.Close:
                        Application.Current.Shutdown();
                        return;
                    case MenuItemType.Manual:
                        var path = Path.Combine(Global.GetBaseFolder(), "Manual.pdf");
                        Process.Start(path);
                        return;
                    default:
                        await _dialogCoordinator.ShowMessageAsync(this, "Error", "The given type is not supported.");
                        break;
                }

                _controlDictionary.Add(type, control);
            }

            if (control == null)
                return;

            ControlDescription = control.Description;
            control.InitControl();

            Control = control;
        }

        /// <summary>
        /// Sets the name of the branch
        /// </summary>
        private void SetBranchName()
        {
            BranchName = $"Branch: {Helper.GetBranchName()}";
        }
    }
}
