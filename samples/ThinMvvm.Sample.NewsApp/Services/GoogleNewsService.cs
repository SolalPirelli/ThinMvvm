using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThinMvvm.Sample.NewsApp.Models;
using Windows.Web.Http;

namespace ThinMvvm.Sample.NewsApp.Services
{
    public sealed class GoogleNewsService : INewsService
    {
        public async Task<NewsFeed> GetFeedAsync()
        {
            using( var client = new HttpClient() )
            {
                var uri = new Uri( "http://news.google.com/news?output=rss", UriKind.Absolute );
                var xml = await client.GetStringAsync( uri );
                return ParseFeed( xml );
            }
        }

        private static NewsFeed ParseFeed( string xml )
        {
            var root = XDocument.Parse( xml ).Root.Element( "channel" );

            return new NewsFeed
            {
                Name = root.Element( "title" ).Value,
                Items = root.Elements( "item" ).Select( ParseItem ).ToArray()
            };
        }

        private static NewsItem ParseItem( XElement elem )
        {
            return new NewsItem
            {
                Title = elem.Element( "title" ).Value,
                Date = DateTimeOffset.Parse( elem.Element( "pubDate" ).Value ),
                Description = elem.Element( "description" ).Value
            };
        }
    }
}