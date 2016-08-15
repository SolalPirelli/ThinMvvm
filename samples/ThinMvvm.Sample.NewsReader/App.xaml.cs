using ThinMvvm.DependencyInjection;
using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsReader.Services;
using ThinMvvm.Sample.NewsReader.ViewModels;
using ThinMvvm.Sample.NewsReader.Views;
using ThinMvvm.Applications;
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

            BackButtonEnabled = true;
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
            StartNavigation( e, ns => ns.NavigateTo<MainViewModel>() );
        }
    }
}