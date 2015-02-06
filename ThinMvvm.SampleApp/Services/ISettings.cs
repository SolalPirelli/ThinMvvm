// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System.Collections.ObjectModel;

namespace ThinMvvm.SampleApp.Services
{
    public interface ISettings
    {
        ObservableCollection<string> ReadArticles { get; set; }
    }
}