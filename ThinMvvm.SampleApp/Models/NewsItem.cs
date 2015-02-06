// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Runtime.Serialization;

namespace ThinMvvm.SampleApp.Models
{
    [DataContract]
    public sealed class NewsItem : ObservableObject
    {
        [DataMember]
        public string Title { get; private set; }

        [DataMember]
        public DateTimeOffset Date { get; private set; }

        [DataMember]
        public string Description { get; private set; }


        private bool _isRead;
        public bool IsRead
        {
            get { return _isRead; }
            set { SetProperty( ref _isRead, value ); }
        }


        public NewsItem( string title, DateTimeOffset date, string description )
        {
            Title = title;
            Date = date;
            Description = description;
        }
    }
}