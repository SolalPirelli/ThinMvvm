using ThinMvvm.DependencyInjection;
using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsReader.Services;
using ThinMvvm.Sample.NewsReader.ViewModels;
using ThinMvvm.Sample.NewsReader.Views;
using ThinMvvm.ViewServices;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Sample.NewsReader
{
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }


        protected override void ConfigureServices( ServiceCollection services )
        {
            base.ConfigureServices( services );

            services.AddSingleton<ILogger, DebugLogger>();
            services.AddTransient<INewsService, GoogleNewsService>();
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