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
            if( Pokemons.Status == DataStatus.None )
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
                var maxIndex = index + 10;

                var pokemons = new List<PokemonInfo>();

                for( int n = index; n < maxIndex && n <= _pokedex.MaxPokemonIndex; n++ )
                {
                    pokemons.Add( await _pokedex.GetPokemonAsync( n ) );
                }

                var nextToken = maxIndex < _pokedex.MaxPokemonIndex ? new Optional<int>( maxIndex ) : default( Optional<int> );

                return new PaginatedData<PokemonInfo, int>( pokemons, nextToken );
            }
        }
    }
}