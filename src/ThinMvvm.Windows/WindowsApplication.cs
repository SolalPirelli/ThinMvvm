using System;
using ThinMvvm.DependencyInjection;
using ThinMvvm.DependencyInjection.Infrastructure;
using ThinMvvm.Applications;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ThinMvvm.Windows.Infrastructure;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Base class for Windows applications.
    /// </summary>
    public abstract class WindowsApplication : Application
    {
        private Type _skeletonType;
        private Type _skeletonModelType;
        private ObjectCreator _objectCreator;
        private WindowsNavigationService _navigationService;
        private bool _backButtonEnabled;


        protected WindowsSplashScreenOptions SplashScreenOptions { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the software back button is enabled.
        /// </summary>
        protected bool BackButtonEnabled
        {
            get { return _backButtonEnabled; }
            set
            {
                _backButtonEnabled = value;
                if( _navigationService != null )
                {
                    UpdateBackButtonVisibility();
                }
            }
        }


        protected WindowsApplication()
        {
            SplashScreenOptions = new WindowsSplashScreenOptions();
        }


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


        /// <summary>
        /// Advanced.
        /// Configures the type of the application skeleton.
        /// 
        /// The default implementation uses a full-size frame with a cache size of 5.
        /// </summary>
        /// <typeparam name="T">The type of the application skeleton.</typeparam>
        protected void ConfigureSkeleton<TViewModel, TView>()
            where TViewModel : ViewModel<NoParameter>
            where TView : FrameworkElement, IWindowsApplicationSkeleton
        {
            if( _navigationService != null )
            {
                throw new InvalidOperationException( $"The skeleton must be configured before running the app." );
            }

            if( _skeletonType != null )
            {
                throw new InvalidOperationException( "The skeleton has already been configured." );
            }

            _skeletonModelType = typeof( TViewModel );
            _skeletonType = typeof( TView );
        }

        /// <summary>
        /// Runs the application.
        /// </summary>
        protected void StartNavigation( IActivatedEventArgs args, Action<INavigationService> navigation )
        {
            Window.Current.Content = Initialize();

            if(args.PreviousExecutionState != ApplicationExecutionState.Terminated||_navigationService.RestorePreviousState())
            {
                navigation( _navigationService );
            }

            if( ShouldActivate( args ) )
            {
                Window.Current.Activate();
            }
        }

        protected void StartOperation<TOperation>( IActivatedEventArgs activationArgs )
            where TOperation : IApplicationOperation
        {
            var root = Initialize();
            var operation = (IApplicationOperation) _objectCreator.Create( typeof( TOperation ), null );
            var shouldActivate = ShouldActivate( activationArgs );

            var splashScreen = new WindowsSplashScreen( _navigationService, operation, activationArgs.SplashScreen, root, shouldActivate, SplashScreenOptions );

            splashScreen.Show();
        }


        private UIElement Initialize()
        {
            // The navigation service obviously needs to be available to ViewModels,
            // but it also needs the object creator, which causes a kind of circular dependency;
            // this cycle is broken by using lazy initialization
            var services = new ServiceCollection();
            services.AddSingleton<INavigationService>( () => _navigationService );
            ConfigureServices( services );
            _objectCreator = services.BuildCreator();

            var viewBinder = new ViewBinder<Page>();
            ConfigureViews( viewBinder );
            var viewRegistry = viewBinder.BuildRegistry();

            FrameworkElement skeleton = null;
            Frame navigationFrame;
            if( _skeletonType == null )
            {
                navigationFrame = new Frame
                {
                    CacheSize = 5
                };
            }
            else
            {
                skeleton = (FrameworkElement) Activator.CreateInstance( _skeletonType );
                navigationFrame = ( (IWindowsApplicationSkeleton) skeleton ).NavigationFrame;
            }

            _navigationService = new WindowsNavigationService( _objectCreator, viewRegistry, navigationFrame );
            _navigationService.Navigated += NavigationServiceNavigated;

            if( skeleton == null )
            {
                return navigationFrame;
            }

            skeleton.DataContext = _objectCreator.Create( _skeletonModelType, null );
            return skeleton;
        }

        private bool ShouldActivate( IActivatedEventArgs activationArgs )
        {
            var prelaunchArgs = activationArgs as IPrelaunchActivatedEventArgs;
            return prelaunchArgs != null && !prelaunchArgs.PrelaunchActivated;
        }

        /// <summary>
        /// Called when the navigation services has navigated to a ViewModel.
        /// </summary>
        private void NavigationServiceNavigated( object sender, NavigatedEventArgs e )
        {
            UpdateBackButtonVisibility();
        }

        /// <summary>
        /// Updates the visibility of the software back button.
        /// </summary>
        private void UpdateBackButtonVisibility()
        {
            if( BackButtonEnabled && _navigationService.CanNavigateBack )
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }
        }
    }
}