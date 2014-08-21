// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.Design;
using ThinMvvm.WindowsPhone.SampleApp.Services.Design;

namespace ThinMvvm.WindowsPhone.SampleApp.ViewModels.Design
{
    public sealed class DesignMainViewModel : DesignViewModel<MainViewModel, int>
    {
#if DEBUG
        protected override MainViewModel ViewModel
        {
            get { return new MainViewModel( new DesignSettings(), new DesignNavigationService(), 1234 ); }
        }
#endif
    }
}