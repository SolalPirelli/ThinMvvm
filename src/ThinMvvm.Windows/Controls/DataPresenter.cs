using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;
using ThinMvvm.Windows.Controls.Infrastructure;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// TODO some architectural problems here.
// Need to distinguish between "a paginated data source was reset"
// and "a paginated data source transformed its items, but must be re-displayed" (i.e. there may be >1 data chunk)
// Might need to revisit the "only Status is worth listening to for changes" assumption.

namespace ThinMvvm.Windows.Controls
{
    /// <summary>
    /// Displays a data source's data.
    /// </summary>
    [TemplatePart( Name = "ContentContainer", Type = typeof( ContentPresenter ) )]
    public class DataPresenter : DataControlBase
    {
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
            DependencyProperty.Register( nameof( DataTemplate ), typeof( DataTemplate ), typeof( DataPresenter ), new PropertyMetadata( null ) );


        /// <summary>
        /// Gets or sets a value indicating whether pagination should be enabled for the source.
        /// 
        /// If this is set to true, the value will be a collection that implements <see cref="ISupportIncrementalLoading" />.
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
            DependencyProperty.Register( nameof( IsPaginating ), typeof( bool ), typeof( DataPresenter ), new PropertyMetadata( false ) );


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
            DependencyProperty.Register( nameof( HideWhenSourceHasNoValue ), typeof( bool ), typeof( DataPresenter ), new PropertyMetadata( false ) );




        private ContentPresenter _contentContainer;


        /// <summary>
        /// Initializes a new instance of the <see cref="DataPresenter" /> class.
        /// </summary>
        public DataPresenter()
        {
            DefaultStyleKey = typeof( DataPresenter );
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
        /// Updates the control.
        /// </summary>
        protected override void Update()
        {
            if( _contentContainer == null || DataSource == null )
            {
                return;
            }

            if( DataSource.Status == DataSourceStatus.Loading && HideWhenSourceHasNoValue )
            {
                _contentContainer.Visibility = Visibility.Collapsed;
            }

            // Only update if the entire data started over
            if( DataSource.Status == DataSourceStatus.Loaded && DataSource.Data.Count == 1 )
            {
                var chunk = DataSource.Data[0];
                if( chunk.Status == DataStatus.Error )
                {
                    _contentContainer.Content = null;

                    if( HideWhenSourceHasNoValue )
                    {
                        _contentContainer.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _contentContainer.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if( IsPaginating )
                    {
                        // Only update the first time
                        if( _contentContainer.Content == null || ( (PaginatedCollectionFromSource) _contentContainer.Content ).Source != DataSource )
                        {
                            _contentContainer.Content = new PaginatedCollectionFromSource( DataSource );
                        }
                    }
                    else
                    {
                        _contentContainer.Content = chunk.Value;
                    }

                    _contentContainer.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Wrapper for a data source that implements <see cref="ISupportIncrementalLoading" />.
        /// </summary>
        private sealed class PaginatedCollectionFromSource : ObservableObject, IList, INotifyCollectionChanged, ISupportIncrementalLoading
        {
            private bool _areValuesLists;
            private IList _items;

            public IDataSource Source { get; }


            public bool HasMoreItems => Source.CanFetchMore;

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
                Source = source;
                Source.PropertyChanged += ( _, e ) =>
                {
                    if( e.PropertyName == nameof( IDataSource.Data ) )
                    {
                        Update();
                    }
                };

                Update();
            }


            public event NotifyCollectionChangedEventHandler CollectionChanged;


            public void Update()
            {
                if( Source.Data[0].Value is IList )
                {
                    _areValuesLists = true;
                    // HACK this won't work with ienumerables of structs
                    _items = Source.Data.SelectMany( d => (IEnumerable<object>) d.Value ).ToList();
                }
                else
                {
                    _areValuesLists = false;
                    _items = Source.Data.Select( d => d.Value ).ToList();
                }

                CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            }

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync( uint count )
            {
                return AsyncInfo.Run( async _ =>
                {
                    await Source.FetchMoreAsync();

                    var newData = Source.Data[Source.Data.Count - 1];
                    if( newData.Status == DataStatus.Error )
                    {
                        return new LoadMoreItemsResult { Count = 0 };
                    }

                    if( _areValuesLists )
                    {
                        var newItems = (IList) newData.Value;
                        for( int n = 0; n < newItems.Count; n++ )
                        {
                            _items.Add( newItems[n] );
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