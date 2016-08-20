using ThinMvvm.Applications;
using ThinMvvm.DependencyInjection;
using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsReader.Services;
using ThinMvvm.Sample.NewsReader.ViewModels;
using ThinMvvm.Sample.NewsReader.Views;
using ThinMvvm.Windows;
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

        protected override WindowsAppConfig ConfigureApp( WindowsAppConfigBuilder builder )
        {
            return builder
                .UseSoftwareBackButton()
                .UseSplashScreen()
                .WithStartupNavigation<MainViewModel>();
        }
    }
}