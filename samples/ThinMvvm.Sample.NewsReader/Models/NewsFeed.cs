using System.Runtime.Serialization;

namespace ThinMvvm.Sample.NewsReader.Models
{
    [DataContract]
    public sealed class NewsFeed
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public NewsItem[] Items { get; private set; }


        public NewsFeed( string name, NewsItem[] items )
        {
            Name = name;
            Items = items;
        }
    }
}