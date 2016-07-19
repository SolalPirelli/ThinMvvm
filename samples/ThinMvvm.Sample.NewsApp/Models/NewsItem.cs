using System;

namespace ThinMvvm.Sample.NewsApp.Models
{
    public sealed class NewsItem
    {
        public string Title { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Description { get; set; }
    }
}