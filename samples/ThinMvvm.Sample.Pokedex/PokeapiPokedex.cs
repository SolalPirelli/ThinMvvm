using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ThinMvvm.Sample.Pokedex
{
    public sealed class PokeapiPokedex : IPokedex
    {
        private readonly HttpClient _client = new HttpClient();


        public int MinPokemonIndex => 1;

        public int MaxPokemonIndex => 721;


        public async Task<PokemonInfo> GetPokemonAsync( int index )
        {
            var json = await _client.GetStringAsync( $"http://pokeapi.co/api/v2/pokemon-species/{index}/" );
            var root = JObject.Parse( json );

            return new PokemonInfo(
                name: root.Value<JArray>( "names" )
                          .First( IsInEnglish )
                          .Value<string>( "name" ),
                pictureUrl: $"http://pokeapi.co/media/sprites/pokemon/{index}.png",
                description: root.Value<JArray>( "flavor_text_entries" )
                                 .First( IsInEnglish )
                                 .Value<string>( "flavor_text" )
                                 .Replace( '\n', ' ' )
            );
        }


        private static bool IsInEnglish( JToken token )
        {
            return token["language"]["name"].Value<string>() == "en";
        }
    }
}