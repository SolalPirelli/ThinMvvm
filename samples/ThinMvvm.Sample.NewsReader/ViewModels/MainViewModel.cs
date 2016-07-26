using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsReader.Models;
using ThinMvvm.Sample.NewsReader.Services;

namespace ThinMvvm.Sample.NewsReader.ViewModels
{
    public sealed class MainViewModel : ViewModel<NoParameter>
    {
        public DataSource<NewsFeed> News { get; }


        public Command<NewsItem> ViewItemCommand { get; }


        public MainViewModel( INavigationService navigationService,
                              IDataStore dataStore,
                              ILogger logger,
                              INewsService newsService )
        {
            News = new BasicDataSource<NewsFeed>( newsService.GetFeedAsync ).WithCache( "Main", dataStore );

            ViewItemCommand = new Command<NewsItem>( navigationService.NavigateTo<ItemViewModel, NewsItem> );

            logger.Register( this, "Main" )
                  .WithCommand( ViewItemCommand, "ViewItem", i => i.Title );

            logger.Register( News );
        }


        protected override async Task OnNavigatedToAsync( NavigationKind navigationKind )
        {
            if( News.Status == DataSourceStatus.None )
            {
                await News.RefreshAsync();
            }
        }
    }
}