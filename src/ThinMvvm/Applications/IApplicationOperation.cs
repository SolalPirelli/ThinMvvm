using System.Threading.Tasks;

namespace ThinMvvm.Applications
{
    public interface IApplicationOperation
    {
        Task ExecuteAsync( INavigationService navigationService );
    }
}