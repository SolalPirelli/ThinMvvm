using ThinMvvm.Logging;
using ThinMvvm.Sample.NewsReader.Models;

namespace ThinMvvm.Sample.NewsReader.ViewModels
{
    public sealed class ItemViewModel : ViewModel<NewsItem>
    {
        public NewsItem Item { get; private set; }


        public ItemViewModel( ILogger logger )
        {
            logger.Register( this, "Item" );
        }


        public override void Initialize( NewsItem arg )
        {
            Item = arg;
        }
    }
}