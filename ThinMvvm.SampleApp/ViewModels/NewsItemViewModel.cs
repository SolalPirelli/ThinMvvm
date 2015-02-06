// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using ThinMvvm.Logging;
using ThinMvvm.SampleApp.Models;

namespace ThinMvvm.SampleApp.ViewModels
{
    [LogId( "NewsItem" )]
    public sealed class NewsItemViewModel : ViewModel<NewsItem>
    {
        public NewsItem Item { get; private set; }

        public NewsItemViewModel( NewsItem item )
        {
            Item = item;
        }
    }
}