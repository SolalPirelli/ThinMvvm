// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Threading;
using ThinMvvm.SampleApp.Models;
using ThinMvvm.SampleApp.Services;

namespace ThinMvvm.SampleApp.ViewModels
{
    public sealed class MainViewModel : CachedDataViewModel<NoParameter, NewsFeed>
    {
        private readonly INavigationService _navigationService;
        private readonly ISettings _settings;
        private readonly INewsService _newsService;


        private NewsFeed _feed;

        public NewsFeed Feed
        {
            get { return _feed; }
            private set { SetProperty( ref _feed, value ); }
        }


        public Command<NewsItem> ViewItemCommand
        {
            get { return this.GetCommand<NewsItem>( ViewItem ); }
        }


        public MainViewModel( IDataCache cache, INavigationService navigationService,
                              ISettings settings, INewsService newsService )
            : base( cache )
        {
            _navigationService = navigationService;
            _settings = settings;
            _newsService = newsService;
        }


        protected override CachedTask<NewsFeed> GetData( bool force, CancellationToken token )
        {
            if ( !force )
            {
                return CachedTask.NoNewData<NewsFeed>();
            }

            return CachedTask.Create( _newsService.GetFeedAsync );
        }

        protected override bool HandleData( NewsFeed data, CancellationToken token )
        {
            if ( data == null )
            {
                // No data yet
                return false;
            }

            foreach ( var item in data.Items )
            {
                if ( _settings.ReadArticles.Contains( item.Title ) )
                {
                    item.IsRead = true;
                }
            }

            Feed = data;

            // No validation, it's always correct
            return true;
        }


        private void ViewItem( NewsItem item )
        {
            if ( !_settings.ReadArticles.Contains( item.Title ) )
            {
                _settings.ReadArticles.Add( item.Title );
                item.IsRead = true;
            }

            _navigationService.NavigateTo<NewsItemViewModel, NewsItem>( item );
        }
    }
}