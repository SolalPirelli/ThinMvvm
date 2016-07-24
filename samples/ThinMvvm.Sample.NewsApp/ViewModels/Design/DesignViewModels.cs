using System;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection;
using ThinMvvm.Sample.NewsApp.Models;
using ThinMvvm.Sample.NewsApp.Services;
using ThinMvvm.Windows;

namespace ThinMvvm.Sample.NewsApp.ViewModels.Design
{
    public sealed class DesignViewModels : DesignViewModelCreator
    {
        public MainViewModel Main => Create<MainViewModel>();

        public ItemViewModel Item => Create<ItemViewModel, NewsItem>( DesignNewsService.FirstItem );


        protected override void ConfigureServices( ServiceCollection services )
        {
            base.ConfigureServices( services );

            services.AddSingleton<INewsService, DesignNewsService>();
        }


        private sealed class DesignNewsService : INewsService
        {
            public static readonly NewsItem FirstItem = new NewsItem
            {
                Title = "Stuff happens",
                Date = DateTimeOffset.Now,
                Description = "<b>Sometimes</b>. But not always."
            };
            public static readonly NewsItem SecondItem = new NewsItem
            {
                Title = "Has Science Gone Too Far? Computer Scientist Creates Fake News Item For Designer Data",
                Date = DateTimeOffset.Now.AddSeconds( -1234567 ),
                Description = "Nah, it's fine."
            };

            public Task<NewsFeed> GetFeedAsync()
            {
                return Task.FromResult( new NewsFeed
                {
                    Name = "Top Stories from Fake News Source",
                    Items = new[]
                    {
                        FirstItem,
                        SecondItem
                    }
                } );
            }
        }
    }
}