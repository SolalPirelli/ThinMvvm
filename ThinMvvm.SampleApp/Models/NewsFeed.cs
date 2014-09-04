// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Runtime.Serialization;

namespace ThinMvvm.SampleApp.Models
{
    [DataContract]
    public sealed class NewsFeed
    {
        [DataMember]
        public string Title { get; private set; }

        [DataMember]
        public DateTimeOffset Date { get; private set; }

        [DataMember]
        public NewsItem[] Items { get; private set; }


        public NewsFeed( string title, DateTimeOffset date, NewsItem[] items )
        {
            Title = title;
            Date = date;
            Items = items;
        }
    }
}