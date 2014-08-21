// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.Design;

namespace ThinMvvm.WindowsPhone.SampleApp.ViewModels.Design
{
    public sealed class DesignAboutViewModel : DesignViewModel<AboutViewModel, NoParameter>
    {
#if DEBUG
        protected override AboutViewModel ViewModel
        {
            get { return new AboutViewModel(); }
        }
#endif
    }
}