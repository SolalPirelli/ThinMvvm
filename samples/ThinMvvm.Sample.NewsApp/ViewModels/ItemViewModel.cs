using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsApp.Models;

namespace ThinMvvm.Sample.NewsApp.ViewModels
{
    public sealed class ItemViewModel : ViewModel<NewsItem>
    {
        public NewsItem Item { get; }


        public ItemViewModel( ILogger logger,
                              NewsItem item )
        {
            Item = item;

            logger.Register( this, "Item" );
        }
    }
}