// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Net;

namespace ThinMvvm.WindowsPhone
{
    /// <summary>
    /// Contains arguments given to an application when it starts.
    /// </summary>
    public sealed class AppArguments
    {
        /// <summary>
        /// Gets the arguments passed in the navigation URI.
        /// </summary>
        public IReadOnlyDictionary<string, string> NavigationArguments { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AppArguments" /> class.
        /// </summary>
        /// <param name="navigationUri">The navigation URI.</param>
        internal AppArguments( Uri navigationUri )
        {
            NavigationArguments = ParseNavigationArguments( navigationUri.ToString() );
        }


        /// <summary>
        /// Parses navigation arguments from an URI (as a string).
        /// </summary>
        private static IReadOnlyDictionary<string, string> ParseNavigationArguments( string uri )
        {
            int index = uri.IndexOf( '?' );
            if ( index == -1 )
            {
                return new Dictionary<string, string>();
            }

            string query = uri.Substring( index + 1 );

            var dic = new Dictionary<string, string>();
            foreach ( var param in query.Split( '&' ) )
            {
                if ( string.IsNullOrWhiteSpace( param ) )
                {
                    continue;
                }

                var parts = param.Split( '=' );
                string key = parts[0].Trim();
                string value = parts.Length > 1 ? HttpUtility.UrlDecode( parts[1] ).Trim() : "";
                dic.Add( key, value );
            }
            return dic;
        }
    }
}