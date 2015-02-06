// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThinMvvm.SampleApp.Models;

namespace ThinMvvm.SampleApp.Services
{
    public sealed class GoogleNewsService : INewsService
    {
        public async Task<NewsFeed> GetFeedAsync()
        {
            string xml = await GetStringAsync( "http://news.google.com/?output=rss" );
            return ParseFeed( xml );
        }

        private static async Task<string> GetStringAsync( string url )
        {
            var req = WebRequest.CreateHttp( url );
            using ( var resp = await Task.Factory.FromAsync( req.BeginGetResponse, (Func<IAsyncResult, WebResponse>) req.EndGetResponse, null ) )
            using ( var stream = resp.GetResponseStream() )
            using ( var reader = new StreamReader( stream ) )
            {
                return await reader.ReadToEndAsync();
            }
        }

        private static NewsFeed ParseFeed( string xml )
        {
            var doc = XDocument.Parse( xml );
            var feedElem = doc.Root.Element( "channel" );
            string title = feedElem.Element( "title" ).Value;
            var date = DateTimeOffset.Parse( feedElem.Element( "pubDate" ).Value );
            var elems = feedElem.Elements( "item" ).Select( ParseItem ).ToArray();

            return new NewsFeed( title, date, elems );
        }

        private static NewsItem ParseItem( XElement elem )
        {
            string title = elem.Element( "title" ).Value;
            var date = DateTimeOffset.Parse( elem.Element( "pubDate" ).Value );
            string description = elem.Element( "description" ).Value;

            return new NewsItem( title, date, description );
        }
    }
}