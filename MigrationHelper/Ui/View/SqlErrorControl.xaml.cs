using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MigrationHelper.DataObjects;

namespace MigrationHelper.Ui.View
{
    /// <summary>
    /// Interaction logic for SqlErrorControl.xaml
    /// </summary>
    public partial class SqlErrorControl
    {
        public delegate void DoubleClickEventHandler(ErrorEntry entry);

        public event DoubleClickEventHandler DoubleClick;

        /// <summary>
        /// Creates a new instance of the <see cref="SqlErrorControl"/>
        /// </summary>
        public SqlErrorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The dependency property of <see cref="ErrorList"/>
        /// </summary>
        public static readonly DependencyProperty ErrorListProperty = DependencyProperty.Register(
            nameof(ErrorList), typeof(ObservableCollection<ErrorEntry>), typeof(SqlErrorControl), new PropertyMetadata(default(ObservableCollection<ErrorEntry>)));

        /// <summary>
        /// Gets or sets the list with the errors
        /// </summary>
        public ObservableCollection<ErrorEntry> ErrorList
        {
            get => (ObservableCollection<ErrorEntry>) GetValue(ErrorListProperty);
            set => SetValue(ErrorListProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected entry
        /// </summary>
        public ErrorEntry SelectedEntry { get; set; }

        /// <summary>
        /// Occurs when the user performs a double click on the a row
        /// </summary>
        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DoubleClick?.Invoke(SelectedEntry);
        }
    }
}
