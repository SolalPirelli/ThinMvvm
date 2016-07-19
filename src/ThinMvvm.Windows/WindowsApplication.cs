using System;
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


        protected virtual WindowsApplicationSkeleton ConfigureSkeleton()
        {
            var frame = new Frame
            {
                CacheSize = 5
            };
            return new WindowsApplicationSkeleton( frame, frame );
        }

        protected virtual void ConfigureServices( ServiceBinder binder )
        {
            binder.Bind<IKeyValueStore>( new WindowsKeyValueStore( null ) );
            binder.Bind<IDataStore>( new WindowsDataStore( null ) );
        }

        protected abstract void ConfigureViews( ViewBinder<Page> binder );


        protected void Initialize()
        {
            if( Window.Current.Content != null )
            {
                return;
            }

            var skeleton = ConfigureSkeleton();
            var serviceBinder = new ServiceBinder();
            var viewBinder = new ViewBinder<Page>();

            _navigationService = new WindowsNavigationService( serviceBinder, viewBinder, skeleton.NavigationFrame );
            _navigationService.Navigated += NavigationServiceNavigated;
            serviceBinder.Bind<INavigationService>( _navigationService );

            ConfigureServices( serviceBinder );
            ConfigureViews( viewBinder );

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