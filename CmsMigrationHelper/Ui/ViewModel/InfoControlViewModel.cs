using System;
using System.Collections.ObjectModel;
using System.Linq;
using CmsMigrationHelper.DataObjects;
using MahApps.Metro.Controls.Dialogs;
using ZimLabs.WpfBase;

namespace CmsMigrationHelper.Ui.ViewModel
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
        /// Backing field for <see cref="OriginalName"/>
        /// </summary>
        private string _originalName;

        /// <summary>
        /// Gets or sets the original name
        /// </summary>
        public string OriginalName
        {
            get => _originalName;
            set => SetField(ref _originalName, value);
        }

        /// <summary>
        /// Backing field for <see cref="FileVersion"/>
        /// </summary>
        private string _fileVersion;

        /// <summary>
        /// Gets or sets the file version
        /// </summary>
        public string FileVersion
        {
            get => _fileVersion;
            set => SetField(ref _fileVersion, value);
        }

        /// <summary>
        /// Backing field for <see cref="ProductVersion"/>
        /// </summary>
        private string _productVersion;

        /// <summary>
        /// Gets or sets the product version
        /// </summary>
        public string ProductVersion
        {
            get => _productVersion;
            set => SetField(ref _productVersion, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator"></param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = new DialogCoordinator();
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
                var (internalName, originalName, fileVersion, productVersion) = Helper.GetVersion();

                Name = internalName;
                OriginalName = originalName;
                FileVersion = fileVersion;
                ProductVersion = productVersion;

                var tmpList = Helper.GetPackageInformation();

                ReferenceList = new ObservableCollection<ReferenceEntry>(tmpList.OrderBy(o => o.Name));

                _loaded = true;
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occured while loading the reference information.\r\n\r\nMessage: {ex.Message}");
            }
        }
    }
}
