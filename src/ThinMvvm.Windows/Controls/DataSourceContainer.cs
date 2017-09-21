using ThinMvvm.Data.Infrastructure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ThinMvvm.Windows.Controls
{
    /// <summary>
    /// Displays all information related to a data source: data, status, and cache status.
    /// </summary>
    public class DataSourceContainer : Control
    {
        /// <summary>
        /// Gets or sets the data source to display.
        /// </summary>
        public IDataSource DataSource
        {
            get { return (IDataSource) GetValue( DataSourceProperty ); }
            set { SetValue( DataSourceProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="DataSource" /> property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register( nameof( DataSource ), typeof( IDataSource ), typeof( DataSourceContainer ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets the template used for the data.
        /// </summary>
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate) GetValue( DataTemplateProperty ); }
            set { SetValue( DataTemplateProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="DataTemplate" /> property.
        /// </summary>
        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register( nameof( DataTemplate ), typeof( DataTemplate ), typeof( DataSourceContainer ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets a value indicating whether pagination should be enabled for the source.
        /// </summary>
        public bool IsPaginating
        {
            get { return (bool) GetValue( IsPaginatingProperty ); }
            set { SetValue( IsPaginatingProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="IsPaginating" /> property.
        /// </summary>
        public static readonly DependencyProperty IsPaginatingProperty =
            DependencyProperty.Register( nameof( IsPaginating ), typeof( bool ), typeof( DataSourceContainer ), new PropertyMetadata( false ) );


        /// <summary>
        /// Gets or sets a value indicating whether to hide the presenter when the source has no value.
        /// </summary>
        public bool HideWhenSourceHasNoValue
        {
            get { return (bool) GetValue( HideWhenSourceHasNoValueProperty ); }
            set { SetValue( HideWhenSourceHasNoValueProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="HideWhenSourceHasNoValue" /> property.
        /// </summary>
        public static readonly DependencyProperty HideWhenSourceHasNoValueProperty =
            DependencyProperty.Register( nameof( HideWhenSourceHasNoValue ), typeof( bool ), typeof( DataSourceContainer ), new PropertyMetadata( false ) );


        /// <summary>
        /// Gets or sets the visibility of the "retry" button.
        /// </summary>
        public Visibility RetryButtonVisibility
        {
            get { return (Visibility) GetValue( RetryButtonVisibilityProperty ); }
            set { SetValue( RetryButtonVisibilityProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="RetryButtonVisibility" /> property.
        /// </summary>
        public static readonly DependencyProperty RetryButtonVisibilityProperty =
            DependencyProperty.Register( nameof( RetryButtonVisibility ), typeof( Visibility ), typeof( DataSourceContainer ), new PropertyMetadata( Visibility.Visible ) );


        /// <summary>
        /// Gets or sets the background color of the cache indicator.
        /// </summary>
        public Brush CacheIndicatorBackground
        {
            get { return (Brush) GetValue( CacheIndicatorBackgroundProperty ); }
            set { SetValue( CacheIndicatorBackgroundProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="CacheIndicatorBackground" /> property.
        /// </summary>
        public static readonly DependencyProperty CacheIndicatorBackgroundProperty =
            DependencyProperty.Register( nameof( CacheIndicatorBackground ), typeof( Brush ), typeof( DataSourceContainer ), new PropertyMetadata( null ) );


        public DataSourceContainer()
        {
            DefaultStyleKey = typeof( DataSourceContainer );
        }
    }
}