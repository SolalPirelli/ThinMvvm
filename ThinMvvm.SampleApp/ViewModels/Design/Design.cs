// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

#if DEBUG
using ThinMvvm.Design;
using ThinMvvm.SampleApp.Services.Design;
#endif

namespace ThinMvvm.SampleApp.ViewModels.Design
{
    public sealed class Design
    {
#if DEBUG
        public MainViewModel Main { get; private set; }
        public NewsItemViewModel NewsItem { get; private set; }

        public Design()
        {
            var newsService = new DesignNewsService();
            var item = newsService.GetFeedAsync().Result.Items[0];
            Main = new MainViewModel( new DesignDataCache(), new DesignNavigationService(), new DesignSettings(), newsService );
            NewsItem = new NewsItemViewModel( item );

            Main.OnNavigatedTo();
            NewsItem.OnNavigatedTo();
        }
#endif
    }
}