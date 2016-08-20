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
            var pokeJson = await _client.GetStringAsync( $"http://pokeapi.co/api/v2/pokemon-species/{index}/" );
            var pokeRoot = JObject.Parse( pokeJson );

            var name = pokeRoot["names"].First( IsInEnglish )
                                        .Value<string>( "name" );

            var description = pokeRoot["flavor_text_entries"].First( IsInEnglish )
                                                             .Value<string>( "flavor_text" )
                                                             .Replace( '\n', ' ' );

            var formJson = await _client.GetStringAsync( $"http://pokeapi.co/api/v2/pokemon-form/{index}/" );
            var formRoot = JObject.Parse( formJson );

            var pictureUrl = formRoot["sprites"].Value<string>( "front_default" );

            return new PokemonInfo( name, pictureUrl, description );
        }


        private static bool IsInEnglish( JToken token )
        {
            return token["language"].Value<string>( "name" ) == "en";
        }
    }
}