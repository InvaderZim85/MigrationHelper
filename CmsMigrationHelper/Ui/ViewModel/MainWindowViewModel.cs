using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CmsMigrationHelper.Ui.View;
using MahApps.Metro.Controls.Dialogs;
using ZimLabs.WpfBase;

namespace CmsMigrationHelper.Ui.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        private object _control;

        public Object Control
        {
            get => _control;
            set => SetField(ref _control, value);
        }

        public ICommand MenuCommand => new RelayCommand<MenuItemType>(SwitchControl);



        private void SwitchControl(MenuItemType type)
        {
            IUserControl control = null;

            switch (type)
            {
                case MenuItemType.Migration:
                    control = new MigrationControl();
                    break;
            }

            if (control == null)
                return;

            control.InitControl();

            Control = control;

        }
    }
}
