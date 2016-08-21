using System;
using System.Threading.Tasks;
using ThinMvvm.Applications.Infrastructure;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Infrastructure;
using ThinMvvm.Windows.Infrastructure;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Bugs and quirks of Windows.UI.Xaml.Controls.Frame:
//
// - There is no way to obtain the current PageStackEntry, only the ones in the back/forward stacks.
//   Thus it's impossible to directly re-inflate the current ViewModel when restoring the frame's state.
//   Workaround: Navigate to a fake page, get the "previous" entry after navigation, then navigate back.
//
// - There is no way to reset it; even with its contents removed, navigation will add an entry to the back stack.
//   Workaround: Mark the current view as transient when resetting the service.
//
// - Navigations cannot occur during a Navigated event.
//   Workaround: Call Task.Yield() in an async void method, to ensure navigation is on a different thread. Yikes!
//
// - There is no way to disable the forward stack.
//   Workaround: Manually clear it after every backwards navigation.
//
// - Pages in the forward stack remain cached (if caching is enabled), even when the forward stack is cleared.
//   In other words, going A -> B -> back -> B will use the same instance of B, causing flickering.
//   Workaround: Manually set the navigation cache mode to Disabled before navigating back.
//
// - If a navigation arg contains the null character ('\0'), the navigation state truncates the string.
//   Not only does this cause data loss, but also weird behavior on the rest of the state.
//   Workaround: Always serialize/deserialize strings when passing them as parameters.
//   This has obvious perf implications, but also ensures that any other bug like this is worked around;
//   attempting to detect null chars and serialize in that case would not provide such a guarantee.

namespace ThinMvvm.Windows
{
    /// <summary>
    /// <see cref="INavigationService" /> implementation for Windows.
    /// </summary>
    /// <remarks>
    /// This class assumes it has complete ownership of the provided frame,
    /// i.e. that nobody else will call any methods on it.
    /// </remarks>
    public sealed class WindowsNavigationService : NavigationServiceBase<Page, string>
    {
        // Sentinel for the state-restoring fake navigation hacks
        private static readonly object NavigationParameterSentinel = new object();

        // Stores the parameter of the current ViewModel in the state-restoring fake navigation hacks
        private object _restoredParameter = NavigationParameterSentinel;

        private readonly Frame _frame;


        /// <summary>
        /// Gets the current depth of the service's back stack.
        /// </summary>
        protected override int BackStackDepth
        {
            get { return _frame.BackStackDepth; }
        }

        /// <summary>
        /// Gets the currently displayed view.
        /// </summary>
        protected override Page CurrentView
        {
            get { return (Page) _frame.Content; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsNavigationService" /> class
        /// with the specified ViewModel creator, views, and navigation frame.
        /// </summary>
        /// <param name="viewModelCreator">The ViewModel creator.</param>
        /// <param name="views">The views.</param>
        /// <param name="savedStateExpirationTime">The expiration time of saved states.</param>
        /// <param name="frame">The navigation frame.</param>
        public WindowsNavigationService( ObjectCreator viewModelCreator, ViewRegistry views,
                                         TimeSpan savedStateExpirationTime, Frame frame )
            : base( viewModelCreator, views, savedStateExpirationTime )
        {
            _frame = frame;

            _frame.Navigating += FrameNavigating;
            _frame.Navigated += FrameNavigated;

            Application.Current.Suspending += ( _, __ ) => StoreState();

            SystemNavigationManager.GetForCurrentView().BackRequested += ( _, e ) =>
            {
                NavigateBack();
                e.Handled = true;
            };
        }


        /// <summary>
        /// Navigates to the specified view, with the specified argument.
        /// </summary>
        protected override void NavigateTo<TArg>( Type viewType, TArg arg )
        {
            var convertedArg = ConvertArgument( arg );
            _frame.Navigate( viewType, convertedArg );
        }

        /// <summary>
        /// Navigates back to the previous ViewModel.
        /// </summary>
        public override void NavigateBack()
        {
            if( CanNavigateBack )
            {
                ( (Page) _frame.Content ).NavigationCacheMode = NavigationCacheMode.Disabled;
                _frame.GoBack();
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
        public override void Reset()
        {
            _frame.BackStack.Clear();
            _frame.Content = null;
            IsCurrentViewTransient = true;
        }

        /// <summary>
        /// Pops the last entry out of the service's back stack.
        /// </summary>
        protected override void PopBackStack()
        {
            _frame.BackStack.RemoveAt( _frame.BackStack.Count - 1 );
        }

        /// <summary>
        /// Gets the current navigation state.
        /// </summary>
        protected override string GetNavigationState()
        {
            return _frame.GetNavigationState();
        }

        /// <summary>
        /// Sets the current navigation state, which may trigger a navigation.
        /// </summary>
        protected override void SetNavigationState( string state )
        {
            _frame.SetNavigationState( state );
            _frame.Navigate( typeof( Page ), NavigationParameterSentinel );
        }

        /// <summary>
        /// Gets the ViewModel of the specified View.
        /// </summary>
        protected override IViewModel GetViewModel( Page view )
        {
            return (IViewModel) view.DataContext;
        }

        /// <summary>
        /// Sets the ViewModel of the specified view.
        /// </summary>
        protected override void SetViewModel( Page view, IViewModel viewModel )
        {
            view.DataContext = viewModel;
        }

        /// <summary>
        /// Gets a state store for the specified name.
        /// </summary>
        protected override IKeyValueStore GetStateStore( string name )
        {
            return new WindowsKeyValueStore( "ThinMvvm.Navigation." + name );
        }


        /// <summary>
        /// Called when the frame is navigating.
        /// </summary>
        private async void FrameNavigating( object sender, NavigatingCancelEventArgs e )
        {
            if( e.Parameter == NavigationParameterSentinel || _restoredParameter != NavigationParameterSentinel )
            {
                return;
            }

            await BeginNavigationAsync( e.NavigationMode == NavigationMode.New ? NavigationKind.Forwards : NavigationKind.Backwards );
        }

        /// <summary>
        /// Called when the frame has finished navigating.
        /// </summary>
        private async void FrameNavigated( object sender, NavigationEventArgs e )
        {
            if( e.Parameter == NavigationParameterSentinel )
            {
                _restoredParameter = _frame.BackStack[_frame.BackStackDepth - 1].Parameter;

                await Task.Yield();

                NavigateBack();
                return;
            }

            var navigationKind = e.NavigationMode == NavigationMode.New ? NavigationKind.Forwards : NavigationKind.Backwards;
            var arg = e.Parameter;

            if( _restoredParameter != NavigationParameterSentinel )
            {
                navigationKind = NavigationKind.Forwards;
                arg = _restoredParameter;

                _restoredParameter = NavigationParameterSentinel;
            }

            var argType = GetParameterType( CurrentView.GetType() );
            arg = ConvertBackArgument( arg, argType );

            await EndNavigationAsync( navigationKind, arg );
        }


        /// <summary>
        /// Converts the specified argument to an object serializable in the Frame's navigation state.
        /// </summary>
        private object ConvertArgument<TArg>( TArg arg )
        {
            if( arg == null )
            {
                return arg;
            }

            if( typeof( TArg ) != typeof( string ) && WindowsSerializer.IsTypeNativelySupported( typeof( TArg ) ) )
            {
                return arg;
            }

            return WindowsSerializer.Serialize( arg );
        }

        /// <summary>
        /// Converts the specified argument back to the specified type.
        /// </summary>
        private object ConvertBackArgument( object arg, Type targetType )
        {
            if( arg == null )
            {
                return arg;
            }

            if( targetType != typeof( string ) && WindowsSerializer.IsTypeNativelySupported( targetType ) )
            {
                return arg;
            }

            return WindowsSerializer.Deserialize( targetType, (string) arg );
        }
    }
}