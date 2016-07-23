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
            System.Diagnostics.Debug.WriteLine( "POKE " + index );

            var url = $"http://pokeapi.co/api/v2/pokemon-species/{index}/";
            var json = await _client.GetStringAsync( url );
            var root = JObject.Parse( json );

            var name = root.Value<JArray>( "names" )
                           .First( IsInEnglish )
                           .Value<string>( "name" );

            var pictureUrl = $"http://pokeapi.co/media/sprites/pokemon/{index}.png";

            var description = root.Value<JArray>( "flavor_text_entries" )
                                  .First( IsInEnglish )
                                  .Value<string>( "flavor_text" )
                                  .Replace( '\n', ' ' );

            return new PokemonInfo( name, pictureUrl, description );
        }


        private static bool IsInEnglish( JToken token )
        {
            return token["language"]["name"].Value<string>() == "en";
        }
    }
}