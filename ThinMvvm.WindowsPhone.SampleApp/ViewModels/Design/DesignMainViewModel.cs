// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.Design;

namespace ThinMvvm.WindowsPhone.SampleApp.ViewModels.Design
{
    public sealed class DesignMainViewModel : DesignViewModel<MainViewModel>
    {
#if DEBUG
        protected override MainViewModel ViewModel
        {
            get { return new MainViewModel( new DesignSettings(), new DesignNavigationService(), 1234 ); }
        }

        private sealed class DesignSettings : ISettings
        {
            public string SavedText
            {
                get { return "Design-only text!"; }
                set { }
            }
        }
#endif
    }
}