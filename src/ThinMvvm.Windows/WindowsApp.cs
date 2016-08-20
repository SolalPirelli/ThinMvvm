using System;
using System.Threading.Tasks;
using ThinMvvm.Applications;
using ThinMvvm.DependencyInjection;
using ThinMvvm.Infrastructure;
using ThinMvvm.Windows.Infrastructure;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    public abstract class WindowsApp : Application
    {
        private IWindowsApplicationSkeleton _skeleton;
        private bool _isBackButtonEnabled;
        private bool _hasNavigated;


        /// <summary>
        /// Configures the application services.
        /// 
        /// The default implementation registers the default Windows implementations of
        /// <see cref="IKeyValueStore" /> and <see cref="IDataStore" />.
        /// </summary>
        /// <param name="services">The service collection.</param>
        protected virtual void ConfigureServices( ServiceCollection services )
        {
            services.AddSingleton<IKeyValueStore>( () => new WindowsKeyValueStore( null ) );
            services.AddSingleton<IDataStore>( () => new WindowsDataStore( null ) );
        }

        /// <summary>
        /// Configures the application views.
        /// </summary>
        /// <param name="binder">The view binder.</param>
        protected abstract void ConfigureViews( ViewBinder<Page> binder );

        protected abstract WindowsAppConfig ConfigureApp( WindowsAppConfigBuilder builder );


        protected override void OnActivated( IActivatedEventArgs args )
        {
            Run( args, ( core, _, a ) => core.ActivateAsync( a ) );
        }

        protected override void OnLaunched( LaunchActivatedEventArgs args )
        {
            Run( args, async ( core, navService, a ) =>
            {
                if( a.PreviousExecutionState == ApplicationExecutionState.Terminated && navService.RestorePreviousState() )
                {
                    await core.ResumeAsync( a );
                }
                else
                {
                    await core.LaunchAsync( a );
                }
            } );
        }


        private async void Run<TArgs>( TArgs args, Func<WindowsAppCore, INavigationService, TArgs, Task> initialization )
            where TArgs : IActivatedEventArgs
        {
            // The navigation service obviously needs to be available to ViewModels,
            // but it also needs the object creator, which causes a kind of circular dependency;
            // this cycle is broken by using lazy initialization
            INavigationService navigationService = null;
            var services = new ServiceCollection();
            services.AddSingleton<INavigationService>( () => navigationService );
            ConfigureServices( services );
            var objectCreator = services.BuildCreator();

            var viewBinder = new ViewBinder<Page>();
            ConfigureViews( viewBinder );
            var viewRegistry = viewBinder.BuildRegistry();

            var configBuilder = new WindowsAppConfigBuilder();
            var config = ConfigureApp( configBuilder );
            _isBackButtonEnabled = config.IsSoftwareBackButtonEnabled;
            _skeleton = config.Skeleton;

            navigationService = new WindowsNavigationService( objectCreator, viewRegistry, _skeleton.NavigationFrame );
            navigationService.Navigated += NavigationServiceNavigated;

            // This is another cycle in the initialization; the navigation service needs the skeleton,
            // but the skeleton's view model (if any) may need the navigation service,
            // thus it has to be initialized later even if that's a bit ugly.
            if( config.SkeletonViewModelType != null )
            {
                ( (FrameworkElement) _skeleton ).DataContext = objectCreator.Create( config.SkeletonViewModelType );
            }

            if( config.SplashScreenGraphics != null )
            {
                var splashScreen = new WindowsSplashScreen( args.SplashScreen, config.SplashScreenGraphics );

                var prelaunchArgs = args as IPrelaunchActivatedEventArgs;
                var shouldActivate = prelaunchArgs != null && !prelaunchArgs.PrelaunchActivated;

                splashScreen.Show( shouldActivate );
            }

            var core = config.CoreFactory( objectCreator );

            await core.InitializeAsync( args );

            await initialization( core, navigationService, args );

            if( !_hasNavigated )
            {
                DismissSplashScreen();
            }
        }

        private void DismissSplashScreen()
        {
            if( Window.Current.Content is WindowsSplashScreen )
            {
                // Safe cast, _config.Skeleton is both an IWindowsApplicationSkeleton and a FrameworkElement
                Window.Current.Content = (FrameworkElement) _skeleton;
                // Activate it in case the splash screen hasn't had time to fully initialize
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Called when the navigation services has navigated to a ViewModel.
        /// </summary>
        private void NavigationServiceNavigated( object sender, NavigatedEventArgs e )
        {
            if( !_hasNavigated )
            {
                e.Target.NavigatedTo += ViewModelFirstNavigatedTo;
            }

            _hasNavigated = true;

            if( _isBackButtonEnabled )
            {
                if( ( (INavigationService) sender ).CanNavigateBack )
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }
                else
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                }
            }
        }

        private void ViewModelFirstNavigatedTo( object sender, EventArgs e )
        {
            ( (IViewModel) sender ).NavigatedTo -= ViewModelFirstNavigatedTo;

            DismissSplashScreen();
        }
    }
}