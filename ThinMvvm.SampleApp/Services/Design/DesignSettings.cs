// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

#if DEBUG
using System.Collections.ObjectModel;
namespace ThinMvvm.SampleApp.Services.Design
{
    public sealed class DesignSettings : ISettings
    {
        public ObservableCollection<string> ReadArticles
        {
            get { return new ObservableCollection<string> { "Obama, Cameron to push for coalition against ISIS at NATO summit - Fox News" }; }
            set { }
        }
    }
}
#endif