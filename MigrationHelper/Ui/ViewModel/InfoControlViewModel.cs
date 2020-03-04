using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using ZimLabs.Utility;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class InfoControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Contains the value which indicates if the data were already loaded
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// Backing field for <see cref="ReferenceList"/>
        /// </summary>
        private ObservableCollection<ReferenceEntry> _referenceList;

        /// <summary>
        /// Contains the list with the reference data
        /// </summary>
        public ObservableCollection<ReferenceEntry> ReferenceList
        {
            get => _referenceList;
            set => SetField(ref _referenceList, value);
        }

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string _name;

        /// <summary>
        /// Gets or sets the name of the application
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// Backing field for <see cref="Version"/>
        /// </summary>
        private string _version;

        /// <summary>
        /// Gets or sets the file version
        /// </summary>
        public string Version
        {
            get => _version;
            set => SetField(ref _version, value);
        }

        /// <summary>
        /// Backing field for <see cref="LogDir"/>
        /// </summary>
        private string _logDir;

        /// <summary>
        /// Gets or sets the path of the log directory
        /// </summary>
        public string LogDir
        {
            get => _logDir;
            set => SetField(ref _logDir, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The instance of the mah apps dialog coordinator</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
        }

        /// <summary>
        /// Loads the data and shows them
        /// </summary>
        public async void LoadData()
        {
            if (_loaded)
                return;

            try
            {
                var (internalName, _, fileVersion, _) = Helper.GetVersion();

                Name = internalName;
                Version = fileVersion;
                LogDir = Path.Combine(Global.GetBaseFolder(), "logs");

                var tmpList = Helper.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(tmpList.OrderBy(o => o.Name));

                _loaded = true;
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occurred while loading the reference information.\r\n\r\nMessage: {ex.Message}");
            }
        }
    }
}
