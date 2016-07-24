using System;
using ThinMvvm.DependencyInjection;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Infrastructure;
using ThinMvvm.Windows.Infrastructure;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// <see cref="INavigationService" /> implementation for Windows.
    /// </summary>
    /// <remarks>
    /// This class assumes it has complete ownership of the provided frame,
    /// i.e. that nobody else will call any methods on it.
    /// </remarks>
    public sealed class WindowsNavigationService : INavigationService
    {
        private const int MaxSuspendedHours = 12;

        // HACK, see NavigateTo(arg)
        private const string SerializedParameterToken = "\0TM_Serialized\0";
        private static readonly string[] SerializedParameterTokenArray = new[] { SerializedParameterToken };

        // HACK, see RestorePreviousState
        private static readonly object FakeNavigationParameter = new object();

        private readonly ObjectCreator _viewModelCreator;
        private readonly IViewRegistry _views;
        private readonly WindowsKeyValueStore _dataStore;
        private readonly Frame _frame;

        private bool _removeCurrentFromBackStack;


        public bool CanNavigateBack
        {
            get { return _frame.BackStackDepth > ( _removeCurrentFromBackStack ? 1 : 0 ); }
        }


        public WindowsNavigationService( ServiceCollection services, IViewRegistry views, Frame frame )
        {
            services.AddInstance<INavigationService>( this );

            _views = views;
            _viewModelCreator = services.BuildCreator();
            _dataStore = new WindowsKeyValueStore( "ThinMvvm.Navigation" );
            _frame = frame;

            _removeCurrentFromBackStack = false;

            _frame.Navigating += FrameNavigating;
            _frame.Navigated += FrameNavigated;

            Application.Current.Suspending += ApplicationSuspending;

            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;
        }


        public event EventHandler<NavigatedEventArgs> Navigated;


        public void NavigateTo<TViewModel>()
            where TViewModel : ViewModel<NoParameter>
        {
            NavigateTo( typeof( TViewModel ), null );
        }

        public void NavigateTo<TViewModel, TParameter>( TParameter arg )
            where TViewModel : ViewModel<TParameter>
        {
            if( WindowsSerializer.IsTypeNativelySupported( typeof( TParameter ) ) )
            {
                NavigateTo( typeof( TViewModel ), arg );
            }
            else
            {
                // HACK
                // Supporting types not natively supported by Windows is a good idea,
                // since this is a common use case; however, in order to know what to
                // deserialize the string to, we also need to store the type!
                // Thus we use a special token that will hopefully never appear anywhere else.
                var serialized = WindowsSerializer.Serialize( arg );
                var typeName = typeof( TParameter ).AssemblyQualifiedName;
                NavigateTo( typeof( TViewModel ), typeName + SerializedParameterToken + serialized );
            }
        }

        public void NavigateBack()
        {
            if( _frame.CanGoBack )
            {
                _frame.GoBack();
                // ThinMvvm, by design, does not support forward navigation
                _frame.ForwardStack.Clear();
            }
            else
            {
                Application.Current.Exit();
            }
        }

        public void Reset()
        {
            _frame.BackStack.Clear();

            if( _frame.Content != null )
            {
                _removeCurrentFromBackStack = true;
            }
        }

        public bool RestorePreviousState()
        {
            try
            {
                var suspendDate = _dataStore.Get<DateTimeOffset>( DataKeys.SuspendDate );
                var navigationState = _dataStore.Get<string>( DataKeys.NavigationState );
                if( !suspendDate.HasValue || !navigationState.HasValue )
                {
                    return false;
                }

                if( ( DateTimeOffset.UtcNow - suspendDate.Value ).TotalHours > MaxSuspendedHours )
                {
                    return false;
                }

                _frame.SetNavigationState( navigationState.Value );

                // MASSIVE HACK
                // There is no way to get the current page's parameter, 
                // as it is neither in the backward nor in the forward stack.
                // Thus, we perform a fake navigation, identified by a special parameter,
                // so that the current page goes in the back stack, which means we can fetch its parameter.
                _frame.Navigate( _frame.SourcePageType, FakeNavigationParameter );
                return true;
            }
            catch
            {
                // If an exception occurs, it's likely to occur again if the restoring was retried

                _dataStore.Delete( DataKeys.SuspendDate );
                _dataStore.Delete( DataKeys.NavigationState );

                return false;
            }
        }


        private void FrameNavigating( object sender, NavigatingCancelEventArgs e )
        {
            // HACK, see RestorePreviousState
            if( e.Parameter == FakeNavigationParameter )
            {
                return;
            }

            if( _frame.Content == null )
            {
                // First navigation
                return;
            }

            var view = (Page) _frame.Content;
            var viewModel = (IViewModel) view.DataContext;

            _removeCurrentFromBackStack = viewModel.IsTransient;

            if( e.NavigationMode == NavigationMode.New )
            {
                var store = GetCurrentStateStore();
                viewModel.SaveState( store );

                viewModel.OnNavigatedFromAsync( NavigationKind.Forwards );
            }
            else
            {
                // No need to save anything.
                // However, to make sure old data doesn't take up disk space forever, delete the current saved data.
                GetCurrentStateStore().Delete();

                viewModel.OnNavigatedFromAsync( NavigationKind.Backwards );
            }
        }

        private void FrameNavigated( object sender, NavigationEventArgs e )
        {
            // HACK, see RestorePreviousState
            if( e.Parameter == FakeNavigationParameter )
            {
                var parameter = _frame.BackStack[_frame.BackStackDepth - 1].Parameter;
                _frame.GoBack();
                EndNavigation( NavigationMode.New, parameter );
                return;
            }

            if( _removeCurrentFromBackStack )
            {
                _frame.BackStack.RemoveAt( _frame.BackStackDepth - 1 ); // not really a "stack"...
                _removeCurrentFromBackStack = false;
            }

            EndNavigation( e.NavigationMode, e.Parameter );
        }

        private void ApplicationSuspending( object sender, SuspendingEventArgs e )
        {
            _dataStore.Set( DataKeys.SuspendDate, DateTimeOffset.UtcNow );
            _dataStore.Set( DataKeys.NavigationState, _frame.GetNavigationState() );

            var view = (Page) _frame.Content;
            var viewModel = (IViewModel) view.DataContext;

            var store = GetCurrentStateStore();
            viewModel.SaveState( store );
        }

        private void BackRequested( object sender, BackRequestedEventArgs e )
        {
            NavigateBack();
            e.Handled = true;
        }


        private void NavigateTo( Type viewModelType, object arg )
        {
            var viewType = _views.GetViewType( viewModelType );
            _frame.Navigate( viewType, arg );
        }

        private void EndNavigation( NavigationMode navigationMode, object arg )
        {
            var view = (Page) _frame.Content;
            if( view.DataContext == null )
            {
                // HACK, see NavigateTo(arg)
                var stringArg = arg as string;
                if( stringArg != null && stringArg.Contains( SerializedParameterToken ) )
                {
                    var parts = stringArg.Split( SerializedParameterTokenArray, 2, StringSplitOptions.None );
                    var type = Type.GetType( parts[0], throwOnError: true );
                    arg = WindowsSerializer.DeserializeUnsafe( type, parts[1] );
                }

                var viewModelType = _views.GetViewModelType( view.GetType() );
                view.DataContext = _viewModelCreator.Create( viewModelType, arg );
            }

            var viewModel = (IViewModel) view.DataContext;
            var store = GetCurrentStateStore();
            if( store.Exists() )
            {
                // If the store exists, loading the state is required.
                viewModel.LoadState( store );
            }

            var navigationKind = navigationMode == NavigationMode.New ? NavigationKind.Forwards : NavigationKind.Backwards;

            Navigated?.Invoke( this, new NavigatedEventArgs( viewModel, navigationKind ) );

            viewModel.OnNavigatedToAsync( navigationKind );
        }

        private WindowsKeyValueStore GetCurrentStateStore()
        {
            return new WindowsKeyValueStore( "ThinMvvm.Navigation." + _frame.BackStackDepth );
        }


        private static class DataKeys
        {
            public const string SuspendDate = "SuspendDate";
            public const string NavigationState = "NavigationState";
        }
    }
}