// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Collections.ObjectModel;

namespace ThinMvvm.SampleApp.Services
{
    public interface ISettings
    {
        ObservableCollection<string> ReadArticles { get; set; }
    }
}