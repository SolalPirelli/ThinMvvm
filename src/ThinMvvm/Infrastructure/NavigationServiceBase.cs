using System;
using System.Reflection;
using System.Threading.Tasks;
using ThinMvvm.Applications.Infrastructure;
using ThinMvvm.DependencyInjection.Infrastructure;

// Important assumption:
//
// Any page can be destroyed at any time.
//
// For instance, the caching limit could be exceeded, caching could be disabled entirely,
// or the page might not have been created at all in an app resume scenario.

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Base class for navigation services.
    /// </summary>
    /// <typeparam name="TView">The base view type.</typeparam>
    /// <typeparam name="TNavigationState">The navigation state type.</typeparam>
    public abstract class NavigationServiceBase<TView, TNavigationState> : INavigationService
    {
        /// <summary>
        /// ODIOUS HACK
        /// TODO: Support dialogs.
        /// </summary>
        public void HACK_PopBackStack()
        {
            PopBackStack();
        }


        // COMPAT: Profile111 does not contain Task.CompletedTask
        private static readonly Task CompletedTask = Task.FromResult( 0 );

        // Name of the suspension store
        private const string SuspensionStoreName = "SuspensionStore";

        // Keys to store the suspend date and navigation state.
        private const string SuspendDateKey = "SuspendDate";
        private const string NavigationStateKey = "NavigationState";


        private readonly ObjectCreator _viewModelCreator;
        private readonly ViewRegistry _views;
        private readonly TimeSpan _navigationStateExpirationTime;


        /// <summary>
        /// Gets the current depth of the service's back stack.
        /// </summary>
        protected abstract int BackStackDepth { get; }

        /// <summary>
        /// Gets the currently displayed view.
        /// </summary>
        protected abstract TView CurrentView { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current view is transient,
        /// i.e. should be removed from the back stack immediately.
        /// </summary>
        protected bool IsCurrentViewTransient { get; set; }

        /// <summary>
        /// Gets a value indicating whether the navigation service can currently navigate back.
        /// </summary>
        public bool CanNavigateBack
        {
            get
            {
                if( IsCurrentViewTransient )
                {
                    return BackStackDepth > 1;
                }

                return BackStackDepth > 0;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationServiceBase{TView, TNavigationState}" /> class,
        /// with the specified ViewModel creator, View registry, and expiration time of stored navigation states.
        /// </summary>
        /// <param name="viewModelCreator">The ViewModel creator.</param>
        /// <param name="views">The View registry.</param>
        /// <param name="savedStateExpirationTime">The expiration time of saved states.</param>
        public NavigationServiceBase( ObjectCreator viewModelCreator, ViewRegistry views, TimeSpan savedStateExpirationTime )
        {
            _viewModelCreator = viewModelCreator;
            _views = views;
            _navigationStateExpirationTime = savedStateExpirationTime;
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
            var viewType = _views.GetViewType( typeof( TViewModel ) );
            NavigateTo<NoParameter>( viewType, null );
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
            var viewType = _views.GetViewType( typeof( TViewModel ) );
            NavigateTo( viewType, arg );
        }

        /// <summary>
        /// Restores navigation state from a previous execution.
        /// The restore can only be successful if the navigation state has not passed its expiration time.
        /// </summary>
        /// <returns>True if state had to be restored and the restore was successful; false otherwise.</returns>
        public bool RestorePreviousState()
        {
            var suspensionStore = GetStateStore( SuspensionStoreName );

            try
            {
                var suspendDate = suspensionStore.Get<DateTimeOffset>( SuspendDateKey );
                var navigationState = suspensionStore.Get<TNavigationState>( NavigationStateKey );
                if( !suspendDate.HasValue || !navigationState.HasValue )
                {
                    return false;
                }

                if( ( DateTimeOffset.UtcNow - suspendDate.Value ) > _navigationStateExpirationTime )
                {
                    return false;
                }

                SetNavigationState( navigationState.Value );

                return true;
            }
            catch
            {
                // If an exception occurs, it's likely to occur again if restoring was retried
                suspensionStore.Clear();

                return false;
            }
        }


        /// <summary>
        /// Navigates to the specified view, with the specified argument.
        /// </summary>
        /// <typeparam name="TArg">The argument type.</typeparam>
        /// <param name="viewType">The view type.</param>
        /// <param name="arg">The argument.</param>
        /// <remarks>
        /// This method is generic so that the argument may be serialized if needed.
        /// </remarks>
        protected abstract void NavigateTo<TArg>( Type viewType, TArg arg );

        /// <summary>
        /// Navigates back to the previous ViewModel.
        /// </summary>
        public abstract void NavigateBack();

        /// <summary>
        /// Resets the service, as if no navigation had occurred.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Pops the last entry out of the service's back stack.
        /// </summary>
        protected abstract void PopBackStack();

        /// <summary>
        /// Gets the current navigation state.
        /// </summary>
        /// <returns>The navigation state.</returns>
        protected abstract TNavigationState GetNavigationState();

        /// <summary>
        /// Sets the current navigation state, which may trigger a navigation.
        /// </summary>
        /// <param name="state">The navigation state.</param>
        protected abstract void SetNavigationState( TNavigationState state );

        /// <summary>
        /// Gets the ViewModel of the specified View.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <returns>The View's ViewModel.</returns>
        protected abstract IViewModel GetViewModel( TView view );

        /// <summary>
        /// Sets the ViewModel of the specified view.
        /// </summary>
        /// <param name="view">The View.</param>
        /// <param name="viewModel">The ViewModel.</param>
        protected abstract void SetViewModel( TView view, IViewModel viewModel );

        /// <summary>
        /// Gets a state store for the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The state store.</returns>
        /// <remarks>
        /// This method must return a <see cref="IKeyValueStore" /> backed by the same storage
        /// if it is called multiple times with the same name.
        /// </remarks>
        protected abstract IKeyValueStore GetStateStore( string name );


        /// <summary>
        /// Asynchronously begins a navigation of the specified kind.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <returns>A task that represents the navigation operation.</returns>
        protected Task BeginNavigationAsync( NavigationKind navigationKind )
        {
            if( CurrentView == null )
            {
                // First navigation
                return CompletedTask;
            }

            var viewModel = GetViewModel( CurrentView );

            if( viewModel.IsTransient )
            {
                IsCurrentViewTransient = true;
            }

            var store = GetStateStore( BackStackDepth.ToString() );
            store.Clear();

            if( navigationKind == NavigationKind.Forwards )
            {
                viewModel.SaveState( store );
            }

            return viewModel.OnNavigatedFromAsync( navigationKind );
        }

        /// <summary>
        /// Asynchronously ends a navigation of the specified kind, with the specified argument.
        /// </summary>
        /// <param name="navigationKind">The navigation kind.</param>
        /// <param name="arg">The navigation argument.</param>
        /// <returns>A task that represents the navigation operation.</returns>
        protected Task EndNavigationAsync( NavigationKind navigationKind, object arg )
        {
            if( IsCurrentViewTransient )
            {
                if( navigationKind == NavigationKind.Forwards && BackStackDepth > 0 )
                {
                    PopBackStack();
                }

                IsCurrentViewTransient = false;
            }

            var viewModel = navigationKind == NavigationKind.Forwards ? null : GetViewModel( CurrentView );
            if( viewModel == null )
            {
                var viewModelType = _views.GetViewModelType( CurrentView.GetType() );
                viewModel = CreateViewModel( viewModelType, arg );

                if( navigationKind == NavigationKind.Backwards || navigationKind == NavigationKind.Restore )
                {
                    var store = GetStateStore( BackStackDepth.ToString() );
                    viewModel.LoadState( store );
                }

                SetViewModel( CurrentView, viewModel );
            }

            Navigated?.Invoke( this, new NavigatedEventArgs( viewModel, navigationKind ) );

            return viewModel.OnNavigatedToAsync( navigationKind );
        }

        /// <summary>
        /// Stores the current state to persistent storage.
        /// </summary>
        protected void StoreState()
        {
            var suspensionStore = GetStateStore( SuspensionStoreName );
            var navigationState = GetNavigationState();
            suspensionStore.Set( NavigationStateKey, navigationState );
            suspensionStore.Set( SuspendDateKey, DateTimeOffset.UtcNow );

            // Can happen if this method is called before any navigation.
            if( CurrentView == null )
            {
                return;
            }

            var viewModel = GetViewModel( CurrentView );
            // Can happen if this method is called in the middle of a navigation,
            // which is acceptable since this is usually called when the app suspends
            if( viewModel == null )
            {
                return;
            }

            var store = GetStateStore( BackStackDepth.ToString() );
            store.Clear();
            viewModel.SaveState( store );
        }

        /// <summary>
        /// Gets the type of the parameter for the ViewModel of the specified View.
        /// </summary>
        /// <remarks>
        /// This method should be used when the platform requires serializing parameters before
        /// navigation can occur, and the original parameter type must be recovered after navigating.
        /// </remarks>
        protected Type GetParameterType( Type viewType )
        {
            var viewModelType = _views.GetViewModelType( viewType );

            while( !viewModelType.IsConstructedGenericType || viewModelType.GetGenericTypeDefinition() != typeof( ViewModel<> ) )
            {
                viewModelType = viewModelType.GetTypeInfo().BaseType;
            }

            return viewModelType.GenericTypeArguments[0];
        }


        /// <summary>
        /// Creates a ViewModel of the specified type, with the specified argument.
        /// </summary>
        /// <param name="type">The ViewModel type.</param>
        /// <param name="arg">The argument.</param>
        /// <returns>The created ViewModel.</returns>
        private IViewModel CreateViewModel( Type type, object arg )
        {
            var viewModel = (IViewModel) _viewModelCreator.Create( type );
            viewModel.Initialize( arg );
            return viewModel;
        }
    }
}