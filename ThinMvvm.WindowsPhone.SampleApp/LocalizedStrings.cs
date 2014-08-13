// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.WindowsPhone.SampleApp.Resources;

namespace ThinMvvm.WindowsPhone.SampleApp
{
    public sealed class LocalizedStrings
    {
        public AppResources Resources { get; private set; }

        public LocalizedStrings()
        {
            Resources = new AppResources();
        }
    }
}