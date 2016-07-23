using System.Threading.Tasks;
using ThinMvvm.Data;
using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsApp.Models;
using ThinMvvm.Sample.NewsApp.Services;

namespace ThinMvvm.Sample.NewsApp.ViewModels
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
            News = new SimpleDataSource<NewsFeed>( newsService.GetFeedAsync ).WithCache( "Main", dataStore );

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