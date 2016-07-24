using ThinMvvm.DependencyInjection;
using ThinMvvm.ViewServices;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Sample.Pokedex
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

            services.AddTransient<IPokedex, PokeapiPokedex>();
        }

        protected override void ConfigureViews( ViewBinder<Page> binder )
        {
            binder.Bind<MainViewModel, MainView>();
        }


        protected override void OnLaunched( LaunchActivatedEventArgs e )
        {
            Initialize();

            NavigationService.NavigateTo<MainViewModel>();

            if( !e.PrelaunchActivated )
            {
                Window.Current.Activate();
            }
        }
    }
}