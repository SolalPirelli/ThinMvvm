#if DEBUG
using ThinMvvm.Design;
using ThinMvvm.SampleApp.WindowsPhone.Services.Design;
using ThinMvvm.SampleApp.WindowsPhone.ViewModels;
#endif

namespace ThinMvvm.SampleApp.ViewModels.Design
{
    public sealed class Design
    {
#if DEBUG
        public MainViewModel MainViewModel { get; private set; }

        public AboutViewModel AboutViewModel { get; private set; }

        public Design()
        {
            MainViewModel = new MainViewModel( new DesignSettings(), new DesignNavigationService(), 1234 );
            AboutViewModel = new AboutViewModel();

            MainViewModel.OnNavigatedTo();
            AboutViewModel.OnNavigatedTo();
        }
#endif
    }
}