using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using ThinMvvm.Data;
using ThinMvvm.Windows.Controls.Infrastructure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace ThinMvvm.Windows.Controls
{
    /// <summary>
    /// Displays a data source or operation's status.
    /// </summary>
    [TemplateVisualState( GroupName = "Data", Name = "None" )]
    [TemplateVisualState( GroupName = "Data", Name = "Loading" )]
    [TemplateVisualState( GroupName = "Data", Name = "LoadingMore" )]
    [TemplateVisualState( GroupName = "Data", Name = "Loaded" )]
    [TemplateVisualState( GroupName = "Data", Name = "Error" )]
    [TemplateVisualState( GroupName = "Data", Name = "NetworkError" )]
    [TemplateVisualState( GroupName = "Cache", Name = "Live" )]
    [TemplateVisualState( GroupName = "Cache", Name = "Cached" )]
    public class DataStatusDisplay : DataControlBase
    {
        /// <summary>
        /// Gets or sets the operation whose status should be displayed.
        /// </summary>
        public DataOperation DataOperation
        {
            get { return (DataOperation) GetValue( DataOperationProperty ); }
            set { SetValue( DataOperationProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="DataOperation" /> property.
        /// </summary>
        public static readonly DependencyProperty DataOperationProperty =
            DependencyProperty.Register( nameof( DataOperation ), typeof( DataOperation ), typeof( DataStatusDisplay ), new PropertyMetadata( null, OnDataOperationChanged ) );

        private static void OnDataOperationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var control = (DataStatusDisplay) d;

            var oldSource = (DataOperation) e.OldValue;
            if( oldSource != null )
            {
                oldSource.PropertyChanged -= control.DataOperationPropertyChanged;
            }

            var source = (DataOperation) e.NewValue;
            if( source != null )
            {
                source.PropertyChanged += control.DataOperationPropertyChanged;
            }

            control.Update();
        }


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
            DependencyProperty.Register( nameof( RetryButtonVisibility ), typeof( Visibility ), typeof( DataStatusDisplay ), new PropertyMetadata( Visibility.Visible ) );


        /// <summary>
        /// Gets or sets the background color of the cached data indicator.
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
            DependencyProperty.Register( nameof( CacheIndicatorBackground ), typeof( Brush ), typeof( DataStatusDisplay ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets the text displayed whenever there is an error.
        /// </summary>
        public string ErrorText
        {
            get { return (string) GetValue( ErrorTextProperty ); }
            set { SetValue( ErrorTextProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="ErrorText" /> property.
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register( nameof( ErrorText ), typeof( string ), typeof( DataStatusDisplay ), new PropertyMetadata( "Error. Try again later." ) );


        /// <summary>
        /// Gets or sets the text displayed whenever there is a network error.
        /// </summary>
        public string NetworkErrorText
        {
            get { return (string) GetValue( NetworkErrorTextProperty ); }
            set { SetValue( NetworkErrorTextProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="NetworkErrorText" /> property.
        /// </summary>
        public static readonly DependencyProperty NetworkErrorTextProperty =
            DependencyProperty.Register( nameof( NetworkErrorText ), typeof( string ), typeof( DataStatusDisplay ), new PropertyMetadata( "Network error. Check your Internet connection." ) );


        /// <summary>
        /// Gets or sets the text displayed when cached data is used.
        /// </summary>
        public string CacheText
        {
            get { return (string) GetValue( CacheTextProperty ); }
            set { SetValue( CacheTextProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="CacheText" /> property.
        /// </summary>
        public static readonly DependencyProperty CacheTextProperty =
            DependencyProperty.Register( nameof( CacheText ), typeof( string ), typeof( DataStatusDisplay ), new PropertyMetadata( "Displaying cached data." ) );




        /// <summary>
        /// Gets a command that will retry the data fetching operation.
        /// </summary>
        public AsyncCommand RetryCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataStatusDisplay" /> class.
        /// </summary>
        public DataStatusDisplay()
        {
            DefaultStyleKey = typeof( DataStatusDisplay );
            RetryCommand = new AsyncCommand( () => DataSource.RefreshAsync() );
        }


        /// <summary>
        /// Indicates whether the specified error is a network error.
        /// </summary>
        /// <param name="error">The error.</param>
        protected virtual bool IsNetworkError( Exception error )
        {
            return error is WebException
                || error is HttpRequestException;
        }


        /// <summary>
        /// Updates the control.
        /// </summary>
        protected override void Update()
        {
            if( DataSource == null )
            {
                VisualStateManager.GoToState( this, "Live", true );

                if( DataOperation == null )
                {
                    VisualStateManager.GoToState( this, "None", true );
                }
                else
                {
                    RetryButtonVisibility = Visibility.Collapsed;

                    UpdateOperationState();
                }
            }
            else
            {
                if( DataOperation == null )
                {
                    UpdateSourceState();
                }
                else
                {
                    throw new InvalidOperationException( $"Cannot set both {nameof( DataSource )} and {nameof( DataOperation )}." );
                }
            }

        }


        /// <summary>
        /// Updates the control's state from the operation.
        /// </summary>
        private void UpdateOperationState()
        {
            if( DataOperation.IsLoading )
            {
                VisualStateManager.GoToState( this, "Loading", true );
            }
            else if( DataOperation.Error == null )
            {
                VisualStateManager.GoToState( this, "None", true );
            }
            else
            {
                var state = IsNetworkError( DataOperation.Error ) ? "NetworkError" : "Error";
                VisualStateManager.GoToState( this, state, true );
            }
        }

        /// <summary>
        /// Updates the control's state from the data source.
        /// </summary>
        private void UpdateSourceState()
        {
            switch( DataSource.Status )
            {
                case DataSourceStatus.Loading:
                    VisualStateManager.GoToState( this, "Loading", true );
                    break;

                case DataSourceStatus.LoadingMore:
                    VisualStateManager.GoToState( this, "LoadingMore", true );
                    break;

                case DataSourceStatus.Loaded:
                    if( DataSource.Data.Any( c => c.Status == DataStatus.Cached ) )
                    {
                        VisualStateManager.GoToState( this, "Cached", true );
                    }
                    else
                    {
                        VisualStateManager.GoToState( this, "Live", true );
                    }

                    var chunk = DataSource.Data.Last();
                    if( chunk.Status == DataStatus.Normal )
                    {
                        VisualStateManager.GoToState( this, "Loaded", true );
                    }
                    else if( chunk.Errors.Fetch != null
                          && chunk.Errors.Cache == null
                          && chunk.Errors.Process == null
                          && IsNetworkError( chunk.Errors.Fetch ) )
                    {
                        VisualStateManager.GoToState( this, "NetworkError", true );
                    }
                    else
                    {
                        VisualStateManager.GoToState( this, "Error", true );
                    }
                    break;

                default:
                    VisualStateManager.GoToState( this, "None", true );
                    break;

            }
        }


        /// <summary>
        /// Called when the operation changes.
        /// </summary>
        private void DataOperationPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == nameof( DataOperation.IsLoading ) )
            {
                Update();
            }
        }
    }
}