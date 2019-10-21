using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace MigrationHelper.Ui.Components
{
    public class CustomDataGrid : DataGrid
    {
        /// <summary>
        /// The dependency property of <see cref="SelectedItemsList"/>
        /// </summary>
        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(CustomDataGrid),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the selected items
        /// </summary>
        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new instance of the <see cref="CustomDataGrid"/>
        /// </summary>
        public CustomDataGrid()
        {
            SelectionChanged += CustomDataGrid_SelectionChanged;
        }

        /// <summary>
        /// Occurs when the selection was changed
        /// </summary>
        private void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItemsList == null)
                return;

            SelectedItemsList.Clear();
            foreach (var selectedItem in SelectedItems)
            {
                SelectedItemsList.Add(selectedItem);
            }
        }
    }
}
