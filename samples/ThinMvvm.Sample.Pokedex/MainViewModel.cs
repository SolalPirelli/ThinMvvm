using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThinMvvm.Data;

namespace ThinMvvm.Sample.Pokedex
{
    public sealed class MainViewModel : ViewModel<NoParameter>
    {
        public PokemonDataSource Pokemons { get; }


        public MainViewModel( IPokedex pokedex, IDataStore dataStore )
        {
            Pokemons = new PokemonDataSource( pokedex, dataStore );
        }


        protected override async Task OnNavigatedToAsync( NavigationKind navigationKind )
        {
            if( Pokemons.Status == DataSourceStatus.None )
            {
                await Pokemons.RefreshAsync();
            }
        }


        public sealed class PokemonDataSource : PaginatedDataSource<PokemonInfo, int>
        {
            private readonly IPokedex _pokedex;


            public PokemonDataSource( IPokedex pokedex, IDataStore dataStore )
            {
                _pokedex = pokedex;

                EnableCache( "Pokemons", dataStore, t => new CacheMetadata( t.HasValue ? t.Value.ToString() : "", null ) );
            }


            protected override async Task<PaginatedData<PokemonInfo, int>> FetchAsync( Optional<int> paginationToken, CancellationToken cancellationToken )
            {
                var index = paginationToken.HasValue ? paginationToken.Value : _pokedex.MinPokemonIndex;
                var pokemon = await _pokedex.GetPokemonAsync( index );
                var nextToken = index < _pokedex.MaxPokemonIndex ? new Optional<int>( index + 1 ) : default( Optional<int> );

                return new PaginatedData<PokemonInfo, int>( pokemon, nextToken );
            }
        }
    }
}