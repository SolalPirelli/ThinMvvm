using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ThinMvvm.Windows.Controls
{
    /// <summary>
    /// Holds data from a source, and displays it along with loading and error information.
    /// </summary>
    [TemplateVisualState( Name = "None" )]
    [TemplateVisualState( Name = "Loading" )]
    [TemplateVisualState( Name = "LoadingMore" )]
    [TemplateVisualState( Name = "Loaded" )]
    [TemplateVisualState( Name = "Cached" )]
    [TemplateVisualState( Name = "Error" )]
    [TemplatePart( Name = "ContentContainer", Type = typeof( ContentPresenter ) )]
    public sealed class DataContainer : Control
    {
        /// <summary>
        /// Gets or sets the template used for values.
        /// 
        /// If the source has list of items as data, this will be used for individual items.
        /// </summary>
        public DataTemplate ValueTemplate
        {
            get { return (DataTemplate) GetValue( ValueTemplateProperty ); }
            set { SetValue( ValueTemplateProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="ValueTemplate" /> property.
        /// </summary>
        public static readonly DependencyProperty ValueTemplateProperty =
            DependencyProperty.Register( nameof( ValueTemplate ), typeof( DataTemplate ), typeof( DataContainer ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        public IDataSource Source
        {
            get { return (IDataSource) GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="Source" /> property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( nameof( Source ), typeof( IDataSource ), typeof( DataContainer ), new PropertyMetadata( null, ContentSourceChanged ) );

        private static void ContentSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var container = (DataContainer) d;

            var oldSource = (IDataSource) e.OldValue;
            if( oldSource != null )
            {
                oldSource.PropertyChanged -= container.ContentSourcePropertyChanged;
            }

            var source = (IDataSource) e.NewValue;
            if( source != null )
            {
                source.PropertyChanged += container.ContentSourcePropertyChanged;
            }

            container.Update();
        }


        /// <summary>
        /// Gets or sets a value indicating whether the raw source value should be displayed.
        /// 
        /// If this is true, the <see cref="ValueTemplate" /> property will be applied to the entire value,
        /// even if the value is a list of items.
        /// </summary>
        public bool DisplayRawValue
        {
            get { return (bool) GetValue( DisplayRawValueProperty ); }
            set { SetValue( DisplayRawValueProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="DisplayRawValue" /> property.
        /// </summary>
        public static readonly DependencyProperty DisplayRawValueProperty =
            DependencyProperty.Register( nameof( DisplayRawValue ), typeof( bool ), typeof( DataContainer ), new PropertyMetadata( false ) );


        /// <summary>
        /// Gets or sets the style for the items container if the value is a list of items.
        /// </summary>
        public Style ItemsContainerStyle
        {
            get { return (Style) GetValue( ItemsContainerStyleProperty ); }
            set { SetValue( ItemsContainerStyleProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="ItemsContainerStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty ItemsContainerStyleProperty =
            DependencyProperty.Register( nameof( ItemsContainerStyle ), typeof( Style ), typeof( DataContainer ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets the text displayed along with the "refresh" button.
        /// </summary>
        public string RefreshText
        {
            get { return (string) GetValue( RefreshTextProperty ); }
            set { SetValue( RefreshTextProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="RefreshText" /> property.
        /// </summary>
        public static readonly DependencyProperty RefreshTextProperty =
            DependencyProperty.Register( nameof( RefreshText ), typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Refresh" ) );


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
            DependencyProperty.Register( nameof( ErrorText ), typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Error while fetching data." ) );


        /// <summary>
        /// Gets or sets the text displayed along with <see cref="ErrorText" /> when the data is cached.
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
            DependencyProperty.Register( nameof( CacheText ), typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Displaying cached data." ) );



        private ContentPresenter _contentContainer;


        /// <summary>
        /// Gets a command that will refresh the data source.
        /// </summary>
        public AsyncCommand RefreshCommand { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataContainer" /> class.
        /// </summary>
        public DataContainer()
        {
            DefaultStyleKey = typeof( DataContainer );
            RefreshCommand = new AsyncCommand( () => Source.RefreshAsync() );
        }


        /// <summary>
        /// Initializes the control after a template has been applied.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            _contentContainer = (ContentPresenter) GetTemplateChild( "ContentContainer" );

            Update();
        }


        /// <summary>
        /// Updates the control according to the content source.
        /// </summary>
        private void Update()
        {
            if( Source == null || Source.Status == DataSourceStatus.None )
            {
                VisualStateManager.GoToState( this, "None", true );
                return;
            }

            switch( Source.Status )
            {
                case DataSourceStatus.Loading:
                    VisualStateManager.GoToState( this, "Loading", true );
                    break;

                case DataSourceStatus.LoadingMore:
                    VisualStateManager.GoToState( this, "LoadingMore", true );
                    break;

                case DataSourceStatus.Loaded:
                    if( Source.Data.Count > 1 )
                    {
                        if( Source.Data.All( d => d.Status == DataStatus.Normal ) )
                        {
                            VisualStateManager.GoToState( this, "Loaded", true );
                        }
                        else if( Source.Data[Source.Data.Count - 1].Status == DataStatus.Error )
                        {
                            // Other chunks can't have errors otherwise a new chunk couldn't have been loaded
                            VisualStateManager.GoToState( this, "Error", true );
                        }
                        else
                        {
                            VisualStateManager.GoToState( this, "Cached", true );
                        }
                    }
                    else
                    {
                        // Initial data, reset stuff
                        var data = Source.Data[0];

                        switch( data.Status )
                        {
                            case DataStatus.Normal:
                                VisualStateManager.GoToState( this, "Loaded", true );
                                break;

                            case DataStatus.Cached:
                                VisualStateManager.GoToState( this, "Cached", true );
                                break;

                            case DataStatus.Error:
                                VisualStateManager.GoToState( this, "Error", true );
                                return; // Nothing to display.
                        }

                        // Show a normal content presenter for non-paginated data that is not a list,
                        // and an listview for everything else. (unless overridden)
                        if( DisplayRawValue || ( !Source.CanFetchMore && !( data.Value is IList ) ) )
                        {
                            _contentContainer.Content = data.Value;
                            _contentContainer.SetBinding( ContentPresenter.ContentTemplateProperty, new Binding
                            {
                                Source = this,
                                Path = new PropertyPath( nameof( ValueTemplate ) )
                            } );
                        }
                        else
                        {
                            var container = new ListView();
                            container.SetBinding( ListView.StyleProperty, new Binding
                            {
                                Source = this,
                                Path = new PropertyPath( nameof( ItemsContainerStyle ) )
                            } );
                            container.SetBinding( ListView.ItemTemplateProperty, new Binding
                            {
                                Source = this,
                                Path = new PropertyPath( nameof( ValueTemplate ) )
                            } );

                            if( Source.CanFetchMore )
                            {
                                container.ItemsSource = new PaginatedCollectionFromSource( Source );
                            }
                            else
                            {
                                container.ItemsSource = data.Value;
                            }

                            _contentContainer.Content = container;
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// Called whenever <see cref="Source" /> has a property change.
        /// </summary>
        private void ContentSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == nameof( IDataSource.Status ) )
            {
                Update();
            }
        }


        /// <summary>
        /// Wrapper for a data source that implements <see cref="ISupportIncrementalLoading" />.
        /// </summary>
        private sealed class PaginatedCollectionFromSource : ObservableObject, IList, INotifyCollectionChanged, ISupportIncrementalLoading
        {
            private readonly IDataSource _source;
            private readonly bool _areValuesLists;
            private readonly IList _items;


            public bool HasMoreItems => _source.CanFetchMore;

            public bool IsFixedSize => false;

            public bool IsReadOnly => true;

            public int Count => _items.Count;

            public bool IsSynchronized => false;

            public object SyncRoot => null;

            public object this[int index]
            {
                get { return _items[index]; }
                set { throw new NotSupportedException(); }
            }


            public PaginatedCollectionFromSource( IDataSource source )
            {
                _source = source;

                var items = source.Data[0].Value as IList;
                if( items == null )
                {
                    _areValuesLists = false;
                    _items = new List<object>() { source.Data[0].Value };
                }
                else
                {
                    _areValuesLists = true;
                    _items = new List<object>( items.Cast<object>() );
                }
            }


            public event NotifyCollectionChangedEventHandler CollectionChanged;


            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync( uint count )
            {
                return AsyncInfo.Run( async _ =>
                {
                    await _source.FetchMoreAsync();

                    var newData = _source.Data[_source.Data.Count - 1];
                    if( newData.Status == DataStatus.Error )
                    {
                        return new LoadMoreItemsResult { Count = 0 };
                    }

                    if( _areValuesLists )
                    {
                        var newItems = (IList) newData.Value;
                        for( int n = 0; n < newItems.Count; n++ )
                        {
                            _items.Add( newItems[0] );
                            var args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, newItems[n], _items.Count - 1 );
                            CollectionChanged?.Invoke( this, args );
                        }

                        return new LoadMoreItemsResult
                        {
                            Count = (uint) newItems.Count
                        };
                    }
                    else
                    {

                        _items.Add( newData.Value );
                        var args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, newData.Value, _items.Count - 1 );
                        CollectionChanged?.Invoke( this, args );

                        return new LoadMoreItemsResult
                        {
                            Count = 1
                        };
                    }
                } );
            }


            public bool Contains( object value ) => _items.Contains( value );

            public int IndexOf( object value ) => _items.IndexOf( value );

            public void CopyTo( Array array, int index ) => _items.CopyTo( array, index );

            public IEnumerator GetEnumerator() => _items.GetEnumerator();

            public int Add( object value )
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public void Insert( int index, object value )
            {
                throw new NotSupportedException();
            }

            public void Remove( object value )
            {
                throw new NotSupportedException();
            }

            public void RemoveAt( int index )
            {
                throw new NotSupportedException();
            }
        }
    }
}