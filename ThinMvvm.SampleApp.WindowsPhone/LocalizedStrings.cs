// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.Resources;

namespace ThinMvvm.SampleApp.WindowsPhone
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