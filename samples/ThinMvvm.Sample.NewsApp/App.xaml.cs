using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsApp.Services;
using ThinMvvm.Sample.NewsApp.ViewModels;
using ThinMvvm.Sample.NewsApp.Views;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Sample.NewsApp
{
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }


        protected override void ConfigureServices( ServiceBinder binder )
        {
            base.ConfigureServices( binder );

            binder.Bind<ILogger, DebugLogger>();
            binder.Bind<INewsService, GoogleNewsService>();
        }

        protected override void ConfigureViews( ViewBinder<Page> binder )
        {
            binder.Bind<MainViewModel, MainView>();
            binder.Bind<ItemViewModel, ItemView>();
        }
        

        protected override void OnLaunched( LaunchActivatedEventArgs e )
        {
            Initialize();
            BackButtonEnabled = true;

            if( e.PreviousExecutionState != ApplicationExecutionState.Terminated || !NavigationService.RestorePreviousState() )
            {
                NavigationService.NavigateTo<MainViewModel>();
            }

            if( !e.PrelaunchActivated )
            {
                Window.Current.Activate();
            }
        }
    }
}