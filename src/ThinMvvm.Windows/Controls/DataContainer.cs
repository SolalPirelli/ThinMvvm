using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using ThinMvvm.Data;
using ThinMvvm.Data.Infrastructure;
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
    [TemplateVisualState( Name = "LoadedUsingCache" )]
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


        public DataSource ContentSource
        {
            get { return (DataSource) GetValue( ContentSourceProperty ); }
            set { SetValue( ContentSourceProperty, value ); }
        }

        public static readonly DependencyProperty ContentSourceProperty =
            DependencyProperty.Register( "ContentSource", typeof( DataSource ), typeof( DataContainer ), new PropertyMetadata( null, ContentSourceChanged ) );

        private static void ContentSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var container = (DataContainer) d;

            var oldSource = (DataSource) e.OldValue;
            if( oldSource != null )
            {
                oldSource.PropertyChanged -= container.DataSourcePropertyChanged;
            }

            var source = (DataSource) e.NewValue;
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
        private object _previousValue;

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
            if( ContentSource == null )
            {
                VisualStateManager.GoToState( this, "None", true );
                return;
            }

            switch( ContentSource.Status )
            {
                case DataStatus.None:
                case DataStatus.NoData:
                    if( ContentSource.LastException == null )
                    {
                        VisualStateManager.GoToState( this, "Loaded", true );
                    }
                    else
                    {
                        VisualStateManager.GoToState( this, "Error", true );
                    }

                    break;

                case DataStatus.Loading:
                    VisualStateManager.GoToState( this, "Loading", true );
                    break;

                case DataStatus.LoadingMore:
                    VisualStateManager.GoToState( this, "LoadingMore", true );
                    break;

                case DataStatus.Loaded:
                    if( ContentSource.CacheStatus == CacheStatus.Unused )
                    {
                        VisualStateManager.GoToState( this, "Loaded", true );
                    }
                    else
                    {
                        VisualStateManager.GoToState( this, "LoadedUsingCache", true );
                    }


                    if( _previousValue != ContentSource.RawValue )
                    {
                        _previousValue = ContentSource.RawValue;

                        if( DisplayRawValue || !( ContentSource.RawValue is IList ) )
                        {
                            _contentContainer.Content = ContentSource.RawValue;
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
                                container.ItemsSource = ContentSource.RawValue;
                            }

                            _contentContainer.Content = container;
                        }
                    }

                    break;
            }
        }


        private void DataSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == nameof( DataSource.Status ) )
            {
                Update();
            }
        }


        private sealed class PaginatedCollectionFromSource : ObservableObject, IList, INotifyCollectionChanged, ISupportIncrementalLoading
        {
            private readonly DataSource _source;
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


            public PaginatedCollectionFromSource( DataSource source )
            {
                _source = source;
                _items = (IList) source.RawValue;
            }


            public event NotifyCollectionChangedEventHandler CollectionChanged;


            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync( uint count )
            {
                return AsyncInfo.Run( async _ =>
                {
                    var oldCount = _items.Count;

                    await _source.FetchMoreAsync();

                    for( int n = oldCount; n < _items.Count; n++ )
                    {
                        var args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, _items[n], n );
                        CollectionChanged?.Invoke( this, args );
                    }

                    return new LoadMoreItemsResult
                    {
                        Count = (uint) ( _items.Count - oldCount )
                    };
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