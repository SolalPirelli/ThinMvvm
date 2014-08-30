// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

#if DEBUG
namespace ThinMvvm.SampleApp.Services.Design
{
    public sealed class DesignSettings : ISettings
    {
        public string SavedText
        {
            get { return "Design text"; }
            set { }
        }
    }
}
#endif