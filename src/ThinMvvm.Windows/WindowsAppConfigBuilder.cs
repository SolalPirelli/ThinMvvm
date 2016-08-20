using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection.Infrastructure;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows
{
    public sealed class WindowsAppConfigBuilder
    {
        private bool _backButtonEnabled;
        private WindowsSplashScreenGraphics _splashScreenGraphics;
        private IWindowsApplicationSkeleton _skeleton;
        private Type _skeletonViewModel;
        private bool _hasCreated;


        public WindowsAppConfigBuilder()
        {
        }


        public WindowsAppConfigBuilder UseSoftwareBackButton()
        {
            EnsureCanBeCalled( !_backButtonEnabled );

            _backButtonEnabled = true;

            return this;
        }

        public WindowsAppConfigBuilder UseSplashScreen( WindowsSplashScreenGraphics graphics = null )
        {
            EnsureCanBeCalled( _splashScreenGraphics == null );

            _splashScreenGraphics = graphics ?? WindowsSplashScreenGraphics.LoadFromAppManifest();

            return this;
        }

        public WindowsAppConfigBuilder UseSkeleton<TView, TViewModel>()
            where TView : FrameworkElement, IWindowsApplicationSkeleton
            where TViewModel : ViewModel<NoParameter>
        {
            EnsureCanBeCalled( _skeleton == null );

            _skeleton = (IWindowsApplicationSkeleton) Activator.CreateInstance( typeof( TView ) );
            _skeletonViewModel = typeof( TViewModel );

            return this;
        }

        public WindowsAppConfig WithStartupNavigation<TViewModel>()
            where TViewModel : ViewModel<NoParameter>
        {
            return WithApplication( creator =>
            {
                var core = (StartupNavigationCore) creator.Create( typeof( StartupNavigationCore ) );
                core.NavigationAction = n => n.NavigateTo<TViewModel>();
                return core;
            } );
        }

        public WindowsAppConfig WithStartupNavigation<TViewModel, TArg>( TArg arg )
            where TViewModel : ViewModel<TArg>
        {
            return WithApplication( creator =>
            {
                var core = (StartupNavigationCore) creator.Create( typeof( StartupNavigationCore ) );
                core.NavigationAction = n => n.NavigateTo<TViewModel, TArg>( arg );
                return core;
            } );
        }

        public WindowsAppConfig WithCore<TApp>()
            where TApp : WindowsAppCore
        {
            return WithApplication( creator => (TApp) creator.Create( typeof( TApp ) ) );
        }


        private WindowsAppConfig WithApplication( Func<ObjectCreator, WindowsAppCore> coreFactory )
        {
            _hasCreated = true;

            return new WindowsAppConfig(
                _backButtonEnabled,
                _splashScreenGraphics,
                _skeleton ?? new DefaultApplicationSkeleton(),
                _skeletonViewModel,
                coreFactory
            );
        }


        private void EnsureCanBeCalled( bool isClear, [CallerMemberName] string methodName = "" )
        {
            if( _hasCreated )
            {
                throw new InvalidOperationException( "The builder has already created a config." );
            }

            if( !isClear )
            {
                throw new InvalidOperationException( methodName + " can only be called once." );
            }
        }

        private sealed class DefaultApplicationSkeleton : Frame, IWindowsApplicationSkeleton
        {
            public Frame NavigationFrame { get; }

            public DefaultApplicationSkeleton()
            {
                CacheSize = 5;
                NavigationFrame = this;
            }
        }

        private sealed class StartupNavigationCore : WindowsAppCore
        {
            private readonly INavigationService _navigationService;

            public Action<INavigationService> NavigationAction { get; set; }


            public StartupNavigationCore( INavigationService navigationService )
            {
                _navigationService = navigationService;
            }


            public override Task LaunchAsync( LaunchActivatedEventArgs args )
            {
                NavigationAction( _navigationService );
                return Task.CompletedTask;
            }
        }
    }
}