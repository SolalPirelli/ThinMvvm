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


        protected override void ConfigureServices( ServiceBinder binder )
        {
            base.ConfigureServices( binder );

            binder.Bind<IPokedex, PokeapiPokedex>();
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