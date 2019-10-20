using System.Windows;
using System.Windows.Controls;

namespace MigrationHelper.Ui.Components
{
    /// <summary>
    /// Interaction logic for Headline.xaml
    /// </summary>
    public partial class Headline : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Headline"/>
        /// </summary>
        public Headline()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The dependency property of <see cref="HeadlineText"/>
        /// </summary>
        public static readonly DependencyProperty HeadlineProperty = DependencyProperty.Register(
            nameof(HeadlineText), typeof(string), typeof(Headline), new PropertyMetadata("Headline"));

        /// <summary>
        /// Gets or sets the text of the headline
        /// </summary>
        public string HeadlineText
        {
            get => (string) GetValue(HeadlineProperty);
            set => SetValue(HeadlineProperty, value);
        }

        /// <summary>
        /// The dependency property of <see cref="IconVisibility"/>
        /// </summary>
        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
            nameof(IconVisibility), typeof(Visibility), typeof(Headline), new PropertyMetadata(default(Visibility)));

        /// <summary>
        /// Gets or sets the icon visibility
        /// </summary>
        public Visibility IconVisibility
        {
            get => (Visibility) GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }
    }
}
