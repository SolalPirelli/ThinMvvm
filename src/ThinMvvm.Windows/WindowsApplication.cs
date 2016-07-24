using System;
using ThinMvvm.DependencyInjection;
using ThinMvvm.ViewServices;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    /// <summary>
    /// Base class for Windows applications.
    /// </summary>
    public abstract class WindowsApplication : Application
    {
        private WindowsNavigationService _navigationService;
        private bool _backButtonEnabled;


        /// <summary>
        /// Gets the navigation service.
        /// This property is only available after calling <see cref="Initialize" />.
        /// </summary>
        protected WindowsNavigationService NavigationService
        {
            get
            {
                if( _navigationService == null )
                {
                    throw new InvalidOperationException(
                        $"The {nameof( NavigationService )} was not initialized."
                      + Environment.NewLine
                      + $"Call {nameof( Initialize )} first."
                    );
                }

                return _navigationService;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the software back button is enabled.
        /// </summary>
        protected bool BackButtonEnabled
        {
            get { return _backButtonEnabled; }
            set
            {
                _backButtonEnabled = value;
                UpdateBackButtonVisibility();
            }
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
        /// Configures the application skeleton.
        /// 
        /// The default implementation uses a full-size frame with a cache size of 5.
        /// </summary>
        /// <returns>The application skeleton.</returns>
        protected virtual WindowsApplicationSkeleton ConfigureSkeleton()
        {
            var frame = new Frame
            {
                CacheSize = 5
            };

            return new WindowsApplicationSkeleton( frame, frame );
        }


        /// <summary>
        /// Initializes the application.
        /// 
        /// This method may be called multiple times, but will ignore any call after the first.
        /// </summary>
        protected void Initialize()
        {
            if( Window.Current.Content != null )
            {
                return;
            }

            var services = new ServiceCollection();
            ConfigureServices( services );

            var viewBinder = new ViewBinder<Page>();
            ConfigureViews( viewBinder );

            var skeleton = ConfigureSkeleton();

            _navigationService = new WindowsNavigationService( services, viewBinder.BuildRegistry(), skeleton.NavigationFrame );
            _navigationService.Navigated += NavigationServiceNavigated;

            Window.Current.Content = skeleton.Root;
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
            if( BackButtonEnabled && NavigationService.CanNavigateBack )
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