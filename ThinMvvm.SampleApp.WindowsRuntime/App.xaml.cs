// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using ThinMvvm.Logging;
using ThinMvvm.SampleApp.Services;
using ThinMvvm.SampleApp.ViewModels;
using ThinMvvm.SampleApp.Views;
using ThinMvvm.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Phone.UI.Input;

namespace ThinMvvm.SampleApp
{
    public sealed partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void Launch( LaunchActivatedEventArgs e )
        {
            // Only do stuff if the app wasn't already open
            if ( e.PreviousExecutionState != ApplicationExecutionState.ClosedByUser
              && e.PreviousExecutionState != ApplicationExecutionState.NotRunning )
            {
                return;
            }

            var navigationService = Container.Bind<IWindowsRuntimeNavigationService, WindowsRuntimeNavigationService>();
            Container.Bind<ISettingsStorage, WindowsRuntimeSettingsStorage>();
            Container.Bind<IDataCache, WindowsRuntimeDataCache>();
            Container.Bind<INewsService, GoogleNewsService>();
            Container.Bind<ISettings, Settings>();

            Container.Bind<Logger, DebugLogger>().Start();

            navigationService.Bind<MainViewModel, MainView>();
            navigationService.Bind<NewsItemViewModel, NewsItemView>();

            // handle the back button since WP doesn't do it for us
            HardwareButtons.BackPressed += ( _, e2 ) =>
            {
                e2.Handled = true;
                navigationService.NavigateBack();
            };

            navigationService.NavigateTo<MainViewModel>();
        }
    }
}