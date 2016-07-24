using System.Runtime.Serialization;

namespace ThinMvvm.Sample.Pokedex
{
    [DataContract]
    public sealed class PokemonInfo
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string PictureUrl { get; private set; }

        [DataMember]
        public string Description { get; private set; }


        public PokemonInfo( string name, string pictureUrl, string description )
        {
            Name = name;
            PictureUrl = pictureUrl;
            Description = description;
        }
    }
}