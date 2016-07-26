using System;
using System.Runtime.Serialization;

namespace ThinMvvm.Sample.NewsReader.Models
{
    [DataContract]
    public sealed class NewsItem
    {
        [DataMember]
        public string Title { get; private set; }

        [DataMember]
        public DateTimeOffset Date { get; private set; }

        [DataMember]
        public string Description { get; private set; }


        public NewsItem( string title, DateTimeOffset date, string description )
        {
            Title = title;
            Date = date;
            Description = description;
        }
    }
}