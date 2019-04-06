using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
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
        private (Action<string> setText, Func<string> getText) _textGetSet;

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
            set => SetField(ref _filename, value);
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
        }
    }
}
