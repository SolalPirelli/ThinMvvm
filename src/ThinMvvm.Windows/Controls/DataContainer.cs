using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ThinMvvm.Data;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace ThinMvvm.Windows.Controls
{
    [TemplateVisualState( Name = "None" )]
    [TemplateVisualState( Name = "Loading" )]
    [TemplateVisualState( Name = "LoadingMore" )]
    [TemplateVisualState( Name = "Loaded" )]
    [TemplateVisualState( Name = "Cached" )]
    [TemplateVisualState( Name = "Error" )]
    [TemplatePart( Name = "ContentContainer", Type = typeof( ContentPresenter ) )]
    public sealed class DataContainer : Control
    {
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate) GetValue( ContentTemplateProperty ); }
            set { SetValue( ContentTemplateProperty, value ); }
        }

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register( "ContentTemplate", typeof( DataTemplate ), typeof( DataContainer ), new PropertyMetadata( null ) );


        public IDataSource ContentSource
        {
            get { return (IDataSource) GetValue( ContentSourceProperty ); }
            set { SetValue( ContentSourceProperty, value ); }
        }

        public static readonly DependencyProperty ContentSourceProperty =
            DependencyProperty.Register( "ContentSource", typeof( IDataSource ), typeof( DataContainer ), new PropertyMetadata( null, ContentSourceChanged ) );

        private static void ContentSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var container = (DataContainer) d;

            var oldSource = (IDataSource) e.OldValue;
            if( oldSource != null )
            {
                oldSource.PropertyChanged -= container.DataSourcePropertyChanged;
            }

            var source = (IDataSource) e.NewValue;
            if( source != null )
            {
                source.PropertyChanged += container.DataSourcePropertyChanged;
            }

            container.Update();
        }



        public bool DisplayRawValue
        {
            get { return (bool) GetValue( DisplayRawValueProperty ); }
            set { SetValue( DisplayRawValueProperty, value ); }
        }

        public static readonly DependencyProperty DisplayRawValueProperty =
            DependencyProperty.Register( "DisplayRawValue", typeof( bool ), typeof( DataContainer ), new PropertyMetadata( false ) );


        public Style ItemsContainerStyle
        {
            get { return (Style) GetValue( ItemsContainerStyleProperty ); }
            set { SetValue( ItemsContainerStyleProperty, value ); }
        }

        public static readonly DependencyProperty ItemsContainerStyleProperty =
            DependencyProperty.Register( "ItemsContainerStyle", typeof( Style ), typeof( DataContainer ), new PropertyMetadata( null ) );



        public string RefreshText
        {
            get { return (string) GetValue( RefreshTextProperty ); }
            set { SetValue( RefreshTextProperty, value ); }
        }

        public static readonly DependencyProperty RefreshTextProperty =
            DependencyProperty.Register( "RefreshText", typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Refresh" ) );


        public string ErrorText
        {
            get { return (string) GetValue( ErrorTextProperty ); }
            set { SetValue( ErrorTextProperty, value ); }
        }

        public static readonly DependencyProperty ErrorTextProperty =
            DependencyProperty.Register( "ErrorText", typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Error while fetching data." ) );


        public string CacheText
        {
            get { return (string) GetValue( CacheTextProperty ); }
            set { SetValue( CacheTextProperty, value ); }
        }

        public static readonly DependencyProperty CacheTextProperty =
            DependencyProperty.Register( "CacheText", typeof( string ), typeof( DataContainer ), new PropertyMetadata( "Displaying cached data." ) );



        private ContentPresenter _contentContainer;

        public AsyncCommand RefreshCommand { get; }


        public DataContainer()
        {
            DefaultStyleKey = typeof( DataContainer );
            RefreshCommand = new AsyncCommand( () => ContentSource.RefreshAsync() );
        }


        protected override void OnApplyTemplate()
        {
            _contentContainer = (ContentPresenter) GetTemplateChild( "ContentContainer" );

            Update();
        }


        private void Update()
        {
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

                case DataSourceStatus.Loaded:
                    if( ContentSource.Data.Count > 1 )
                    {
                        if( ContentSource.Data.All( d => d.Status == DataStatus.Normal ) )
                        {
                            VisualStateManager.GoToState( this, "Loaded", true );
                        }
                        else if( ContentSource.Data[ContentSource.Data.Count - 1].Status == DataStatus.Error )
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
                        var data = ContentSource.Data[0];

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
                        if( DisplayRawValue || ( !ContentSource.CanFetchMore && !( data.Value is IList ) ) )
                        {
                            _contentContainer.Content = data.Value;
                            _contentContainer.SetBinding( ContentPresenter.ContentTemplateProperty, new Binding
                            {
                                Source = this,
                                Path = new PropertyPath( nameof( ContentTemplate ) )
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
                                Path = new PropertyPath( nameof( ContentTemplate ) )
                            } );

                            if( ContentSource.CanFetchMore )
                            {
                                container.ItemsSource = new PaginatedCollectionFromSource( ContentSource );
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


        private void DataSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == nameof( IDataSource.Status ) )
            {
                Update();
            }
        }


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