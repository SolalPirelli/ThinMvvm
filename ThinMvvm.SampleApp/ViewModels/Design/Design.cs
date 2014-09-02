#if DEBUG
using ThinMvvm.Design;
using ThinMvvm.SampleApp.Services.Design;
#endif

namespace ThinMvvm.SampleApp.ViewModels.Design
{
    public sealed class Design
    {
#if DEBUG
        public MainViewModel Main { get; private set; }

        public AboutViewModel About { get; private set; }

        public Design()
        {
            Main = new MainViewModel( new DesignSettings(), new DesignNavigationService(), 1234 );
            About = new AboutViewModel();

            Main.OnNavigatedTo();
            About.OnNavigatedTo();
        }
#endif
    }
}