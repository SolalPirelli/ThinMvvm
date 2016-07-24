using System.Threading.Tasks;
using ThinMvvm.Infrastructure;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class ViewModelTests
    {
        private sealed class MyViewModel : ViewModel<NoParameter> { }

        [Fact]
        public void NotTransientByDefault()
        {
            var vm = (IViewModel) new MyViewModel();

            Assert.False( vm.IsTransient );
        }

        [Fact]
        public async Task OnNavigatedToAsyncFiresNavigatedToEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            vm.NavigatedTo += ( _, e ) => count++;

            await vm.OnNavigatedToAsync( NavigationKind.Forwards );

            Assert.Equal( 1, count );
        }

        [Fact]
        public async Task OnNavigatedFromAsyncFiresNavigatedFromEvent()
        {
            var vm = (IViewModel) new MyViewModel();

            int count = 0;
            vm.NavigatedFrom += ( _, e ) => count++;

            await vm.OnNavigatedFromAsync( NavigationKind.Forwards );

            Assert.Equal( 1, count );
        }
    }
}