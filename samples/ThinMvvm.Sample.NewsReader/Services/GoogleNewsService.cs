using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThinMvvm.Sample.NewsReader.Models;
using Windows.Web.Http;

namespace ThinMvvm.Sample.NewsReader.Services
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

            return new NewsFeed(
                name: root.Element( "title" ).Value,
                items: root.Elements( "item" ).Select( ParseItem ).ToArray()
            );
        }

        private static NewsItem ParseItem( XElement elem )
        {
            return new NewsItem(
                title: elem.Element( "title" ).Value,
                date: DateTimeOffset.Parse( elem.Element( "pubDate" ).Value ),
                description: elem.Element( "description" ).Value
            );
        }
    }
}