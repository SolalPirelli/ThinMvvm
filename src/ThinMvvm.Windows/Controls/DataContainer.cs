using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    [TemplateVisualState( GroupName = "Data", Name = "None" )]
    [TemplateVisualState( GroupName = "Data", Name = "Loading" )]
    [TemplateVisualState( GroupName = "Data", Name = "LoadingMore" )]
    [TemplateVisualState( GroupName = "Data", Name = "Transforming" )]
    [TemplateVisualState( GroupName = "Data", Name = "Loaded" )]
    [TemplateVisualState( GroupName = "Data", Name = "Error" )]
    [TemplateVisualState( GroupName = "Data", Name = "NetworkError" )]
    [TemplateVisualState( GroupName = "Cache", Name = "NotCached" )]
    [TemplateVisualState( GroupName = "Cache", Name = "Cached" )]
    [TemplatePart( Name = "ContentContainer", Type = typeof( ContentPresenter ) )]
    public class DataContainer : Control
    {
        /// <summary>
        /// Gets or sets the template used for the content.
        /// </summary>
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate) GetValue( ContentTemplateProperty ); }
            set { SetValue( ContentTemplateProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="ContentTemplate" /> property.
        /// </summary>
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register( nameof( ContentTemplate ), typeof( DataTemplate ), typeof( DataContainer ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets the source of the content.
        /// </summary>
        public IDataSource ContentSource
        {
            get { return (IDataSource) GetValue( ContentSourceProperty ); }
            set { SetValue( ContentSourceProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="ContentSource" /> property.
        /// </summary>
        public static readonly DependencyProperty ContentSourceProperty =
            DependencyProperty.Register( nameof( ContentSource ), typeof( IDataSource ), typeof( DataContainer ), new PropertyMetadata( null, ContentSourceChanged ) );

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
        /// Gets or sets a value indicating whether pagination should be enabled for the source.
        /// 
        /// If this is set to true, the value will be a collection that implements <see cref="ISupportIncrementalLoading" />.
        /// </summary>
        public bool EnablePagination
        {
            get { return (bool) GetValue( EnablePaginationProperty ); }
            set { SetValue( EnablePaginationProperty, value ); }
        }

        public static readonly DependencyProperty EnablePaginationProperty =
            DependencyProperty.Register( nameof( EnablePagination ), typeof( bool ), typeof( DataContainer ), new PropertyMetadata( false ) );


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


        public string NetworkErrorText
        {
            get { return (string) GetValue( NetworkErrorTextProperty ); }
            set { SetValue( NetworkErrorTextProperty, value ); }
        }

        public static readonly DependencyProperty NetworkErrorTextProperty =
            DependencyProperty.Register( nameof( NetworkErrorText ), typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Network error. Check your Internet connection." ) );


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
            RefreshCommand = new AsyncCommand( () => ContentSource.RefreshAsync() );
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
        /// Indicates whether the specified error is a network error.
        /// </summary>
        /// <param name="error">The error.</param>
        protected virtual bool IsNetworkError( Exception error )
        {
            return error is WebException
                || error is HttpRequestException;
        }


        /// <summary>
        /// Updates the control according to the content source.
        /// </summary>
        private void Update()
        {
            if( _contentContainer == null )
            {
                return;
            }

            if( ContentSource == null || ContentSource.Status == DataSourceStatus.None )
            {
                VisualStateManager.GoToState( this, "None", true );
                return;
            }

            switch( ContentSource.Status )
            {
                case DataSourceStatus.Loading:
                    VisualStateManager.GoToState( this, "Loading", true );
                    break;

                case DataSourceStatus.LoadingMore:
                    VisualStateManager.GoToState( this, "LoadingMore", true );
                    break;

                case DataSourceStatus.Transforming:
                    VisualStateManager.GoToState( this, "Transforming", true );
                    break;

                case DataSourceStatus.Loaded:
                    if( ContentSource.Data.Count > 1 )
                    {
                        // Just an update, use existing data containwr

                        if( ContentSource.Data.All( d => d.Status == DataStatus.Normal ) )
                        {
                            VisualStateManager.GoToState( this, "Loaded", true );
                        }
                        else
                        {
                            // Other chunks can't have errors otherwise a new chunk couldn't have been loaded
                            GoToChunkStates( ContentSource.Data[ContentSource.Data.Count - 1] );
                        }
                    }
                    else
                    {
                        // Initial data, reset stuff

                        var data = ContentSource.Data[0];

                        GoToChunkStates( data );

                        if( data.Status == DataStatus.Error )
                        {
                            _contentContainer.Content = null;
                            return;
                        }

                        if( EnablePagination )
                        {
                            _contentContainer.Content = new PaginatedCollectionFromSource( ContentSource );
                        }
                        else
                        {
                            _contentContainer.Content = data.Value;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Go to the appropriate visual states for the specified data chunk.
        /// </summary>
        private void GoToChunkStates( IDataChunk chunk )
        {
            if( chunk.Status == DataStatus.Normal )
            {
                VisualStateManager.GoToState( this, "Loaded", true );
                VisualStateManager.GoToState( this, "NotCached", true );
                return;
            }

            if( chunk.Errors.Fetch != null
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

            if( chunk.Status == DataStatus.Cached )
            {
                VisualStateManager.GoToState( this, "Cached", true );
            }
            else
            {
                VisualStateManager.GoToState( this, "NotCached", true );
            }
        }

        /// <summary>
        /// Called whenever <see cref="ContentSource" /> has a property change.
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