using System.Threading.Tasks;

namespace ThinMvvm.Sample.Pokedex
{
    public interface IPokedex
    {
        int MinPokemonIndex { get; }
        int MaxPokemonIndex { get; }

        Task<PokemonInfo> GetPokemonAsync( int index );
    }
}