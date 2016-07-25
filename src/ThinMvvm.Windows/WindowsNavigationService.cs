using System;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Infrastructure;
using ThinMvvm.ViewServices.Infrastructure;
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
    /// 
    /// Does not support asynchronous navigation.
    /// </summary>
    /// <remarks>
    /// This class assumes it has complete ownership of the provided frame,
    /// i.e. that nobody else will call any methods on it.
    /// </remarks>
    public sealed class WindowsNavigationService : INavigationService
    {
        private const int MaxSuspendedHours = 12;

        // HACK, see NavigateTo(arg)
        private const string SerializedParameterToken = "TM_Serialized";
        private static readonly string[] SerializedParameterTokenArray = new[] { SerializedParameterToken };

        // HACK, see RestorePreviousState
        private static readonly object NavigationParameterSentinel = new object();
        private object _restoredParameter = NavigationParameterSentinel;

        private readonly ObjectCreator _viewModelCreator;
        private readonly ViewRegistry _views;
        private readonly WindowsKeyValueStore _dataStore;
        private readonly Frame _frame;

        private bool _removeCurrentFromBackStack;


        /// <summary>
        /// Gets a value indicating whether the navigation service can currently navigate back.
        /// </summary>
        public bool CanNavigateBack
        {
            get { return _frame.BackStackDepth > ( _removeCurrentFromBackStack ? 1 : 0 ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsNavigationService" /> class
        /// with the specified services, views, and navigation frame.
        /// 
        /// The navigation service will add itself to the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="views">The views.</param>
        /// <param name="frame">The navigation frame.</param>
        public WindowsNavigationService( ServiceCollection services, ViewRegistry views, Frame frame )
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


        /// <summary>
        /// Occurs after navigating to a ViewModel.
        /// </summary>
        public event EventHandler<NavigatedEventArgs> Navigated;


        /// <summary>
        /// Navigates to the specified parameterless ViewModel type.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        public void NavigateTo<TViewModel>()
            where TViewModel : ViewModel<NoParameter>
        {
            NavigateTo( typeof( TViewModel ), null );
        }

        /// <summary>
        /// Navigates to the specified ViewModel type using the specified argument.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TArg">The argument type.</typeparam>
        /// <param name="arg">The argument.</param>
        public void NavigateTo<TViewModel, TArg>( TArg arg )
            where TViewModel : ViewModel<TArg>
        {
            if( WindowsSerializer.IsTypeNativelySupported( typeof( TArg ) ) )
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
                var typeName = typeof( TArg ).AssemblyQualifiedName;
                NavigateTo( typeof( TViewModel ), typeName + SerializedParameterToken + serialized );
            }
        }

        /// <summary>
        /// Navigates back to the previous ViewModel.
        /// </summary>
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

        /// <summary>
        /// Resets the service, as if no navigation had occurred.
        /// </summary>
        public void Reset()
        {
            _frame.BackStack.Clear();

            if( _frame.Content != null )
            {
                _removeCurrentFromBackStack = true;
            }
        }

        /// <summary>
        /// Restores navigation state from a previous app execution.
        /// </summary>
        /// <returns>True if state had to be restored and the restore was successful; false otherwise.</returns>
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
                // Then, we remember said parameter and navigate back, and finally finish navigation as usual.
                _frame.Navigate( _frame.SourcePageType, NavigationParameterSentinel );
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


        /// <summary>
        /// Called when the frame is navigating.
        /// </summary>
        private void FrameNavigating( object sender, NavigatingCancelEventArgs e )
        {
            // HACK, see RestorePreviousState
            if( e.Parameter == NavigationParameterSentinel || _restoredParameter != NavigationParameterSentinel )
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
                store.Clear();
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

        /// <summary>
        /// Called when the frame has finished navigating.
        /// </summary>
        private async void FrameNavigated( object sender, NavigationEventArgs e )
        {
            // HACK, see RestorePreviousState
            if( e.Parameter == NavigationParameterSentinel )
            {
                _restoredParameter = _frame.BackStack[_frame.BackStackDepth - 1].Parameter;
                // Frame ignores navigations that occur during the Navigated event.
                // This is not pretty!
                await Task.Yield();
                NavigateBack();
                return;
            }
            if( _restoredParameter != NavigationParameterSentinel )
            {
                EndNavigation( NavigationMode.New, _restoredParameter );
                _restoredParameter = NavigationParameterSentinel;
                return;
            }

            if( _removeCurrentFromBackStack )
            {
                _frame.BackStack.RemoveAt( _frame.BackStackDepth - 1 ); // not really a "stack"...
                _removeCurrentFromBackStack = false;
            }

            EndNavigation( e.NavigationMode, e.Parameter );
        }

        /// <summary>
        /// Called when the app is about to be suspended.
        /// </summary>
        private void ApplicationSuspending( object sender, SuspendingEventArgs e )
        {
            _dataStore.Set( DataKeys.SuspendDate, DateTimeOffset.UtcNow );
            _dataStore.Set( DataKeys.NavigationState, _frame.GetNavigationState() );

            var view = (Page) _frame.Content;
            var viewModel = (IViewModel) view.DataContext;

            var store = GetCurrentStateStore();
            store.Clear();
            viewModel.SaveState( store );
        }

        /// <summary>
        /// Called when the user pressed the (hardware or software) back button.
        /// </summary>
        private void BackRequested( object sender, BackRequestedEventArgs e )
        {
            NavigateBack();
            e.Handled = true;
        }


        /// <summary>
        /// Navigates to the specified ViewModel type using the specified argument.
        /// </summary>
        private void NavigateTo( Type viewModelType, object arg )
        {
            var viewType = _views.GetViewType( viewModelType );
            _frame.Navigate( viewType, arg );
        }

        /// <summary>
        /// Ends a navigation by storing necessary data and creating a ViewModel if necessary.
        /// </summary>
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
                    arg = WindowsSerializer.Deserialize( type, parts[1] );
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

        /// <summary>
        /// Gets the current store, which changes at each navigation.
        /// </summary>
        private WindowsKeyValueStore GetCurrentStateStore()
        {
            return new WindowsKeyValueStore( "ThinMvvm.Navigation." + _frame.BackStackDepth );
        }


        /// <summary>
        /// Holds string constants to use as keys for storage.
        /// </summary>
        private static class DataKeys
        {
            public const string SuspendDate = "SuspendDate";
            public const string NavigationState = "NavigationState";
        }
    }
}