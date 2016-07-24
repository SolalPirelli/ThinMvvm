using System;
using System.Runtime.Serialization;

namespace ThinMvvm.Sample.NewsApp.Models
{
    [DataContract]
    public sealed class NewsItem
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public DateTimeOffset Date { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}