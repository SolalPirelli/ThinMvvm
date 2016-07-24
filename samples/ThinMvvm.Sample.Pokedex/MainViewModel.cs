using System.Threading.Tasks;
using ThinMvvm.Data;

namespace ThinMvvm.Sample.Pokedex
{
    public sealed class MainViewModel : ViewModel<NoParameter>
    {
        public PaginatedDataSource<PokemonInfo, int> Pokemons { get; }


        public MainViewModel( IPokedex pokedex, IDataStore dataStore )
        {
            Pokemons = new BasicPaginatedDataSource<PokemonInfo, int>( async token =>
            {
                var index = token.HasValue ? token.Value : pokedex.MinPokemonIndex;
                var pokemon = await pokedex.GetPokemonAsync( index );
                var nextToken = index < pokedex.MaxPokemonIndex ? new Optional<int>( index + 1 ) : default( Optional<int> );

                return new PaginatedData<PokemonInfo, int>( pokemon, nextToken );
            } ).WithCache( "Pokemons", dataStore, t => new CacheMetadata( t.HasValue ? t.Value.ToString() : "", null ) );
        }


        protected override async Task OnNavigatedToAsync( NavigationKind navigationKind )
        {
            if( Pokemons.Status == DataSourceStatus.None )
            {
                await Pokemons.RefreshAsync();
            }
        }
    }
}