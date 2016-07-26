using System;
using System.Threading.Tasks;
using ThinMvvm.DependencyInjection;
using ThinMvvm.Sample.NewsReader.Models;
using ThinMvvm.Sample.NewsReader.Services;
using ThinMvvm.Windows;

namespace ThinMvvm.Sample.NewsReader.ViewModels.Design
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
            public static readonly NewsItem FirstItem = new NewsItem(
                title: "Stuff happens",
                date: DateTimeOffset.Now,
                description: "<b>Sometimes</b>. But not always."
            );

            public static readonly NewsItem SecondItem = new NewsItem(
                title: "Has Science Gone Too Far? Computer Scientist Creates Fake News Item For Designer Data",
                date: DateTimeOffset.Now.AddSeconds( -1234567 ),
                description: "Nah, it's fine."
            );

            public Task<NewsFeed> GetFeedAsync()
            {
                return Task.FromResult( new NewsFeed(
                    name: "Top Stories from Fake News Source",
                    items: new[] { FirstItem, SecondItem }
                ) );
            }
        }
    }
}