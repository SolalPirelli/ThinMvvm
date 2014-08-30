// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ThinMvvm.WindowsRuntime.Internals
{
    public static class Serializer
    {
        // DO NOT CHANGE
        private static readonly Encoding Encoding = Encoding.Unicode;


        public static string Serialize( object value )
        {
            using ( var stream = new MemoryStream() )
            {
                new DataContractJsonSerializer( value.GetType() ).WriteObject( stream, value );
                return Encoding.GetString( stream.ToArray(), 0, (int) stream.Length );
            }
        }

        public static T Deserialize<T>( string serialized )
        {
            byte[] serializedBytes = Encoding.GetBytes( serialized );
            using ( var stream = new MemoryStream( serializedBytes ) )
            {
                return (T) new DataContractJsonSerializer( typeof( T ) ).ReadObject( stream );
            }
        }
    }
}