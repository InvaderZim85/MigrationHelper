using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.Ui.View;
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
        private string _controlDescription = "CMS migration helper";

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
        private string _title = "CMS Migration Helper";

        /// <summary>
        /// Gets or sets the title of the main window
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;

            var version = Helper.GetVersion();

            Title = $"CMS Migration Helper (v{version.productVersion})";

            SwitchControl(MenuItemType.Migration);
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
                    case MenuItemType.Info:
                        control = new InfoControl();
                        break;
                    case MenuItemType.Close:
                        Application.Current.Shutdown();
                        break;
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
    }
}
