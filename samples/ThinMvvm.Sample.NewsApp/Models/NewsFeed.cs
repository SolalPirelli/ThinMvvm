namespace ThinMvvm.Sample.NewsApp.Models
{
    public sealed class NewsFeed
    {
        public string Name { get; set; }

        public NewsItem[] Items { get; set; }
    }
}