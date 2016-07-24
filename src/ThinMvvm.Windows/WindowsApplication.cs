using System;
using ThinMvvm.DependencyInjection;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    public abstract class WindowsApplication : Application
    {
        private WindowsNavigationService _navigationService;
        private bool _backButtonEnabled;


        protected WindowsNavigationService NavigationService
        {
            get
            {
                if( _navigationService == null )
                {
                    throw new InvalidOperationException(
                        "The navigation service was not initialized."
                      + Environment.NewLine
                      + "Call Initialize() first."
                    );
                }

                return _navigationService;
            }
        }

        protected bool BackButtonEnabled
        {
            get { return _backButtonEnabled; }
            set
            {
                _backButtonEnabled = value;
                UpdateBackButtonVisibility();
            }
        }


        protected virtual void ConfigureServices( ServiceCollection services )
        {
            services.AddSingleton<IKeyValueStore>( () => new WindowsKeyValueStore( null ) );
            services.AddSingleton<IDataStore>( () => new WindowsDataStore( null ) );
        }

        protected abstract void ConfigureViews( ViewBinder<Page> binder );

        protected virtual WindowsApplicationSkeleton ConfigureSkeleton()
        {
            var frame = new Frame
            {
                CacheSize = 5
            };

            return new WindowsApplicationSkeleton( frame, frame );
        }


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

            _navigationService = new WindowsNavigationService( services, viewBinder, skeleton.NavigationFrame );
            _navigationService.Navigated += NavigationServiceNavigated;

            Window.Current.Content = skeleton.Root;
        }


        private void NavigationServiceNavigated( object sender, NavigatedEventArgs e )
        {
            UpdateBackButtonVisibility();
        }
        
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