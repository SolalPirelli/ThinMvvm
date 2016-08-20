using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace ThinMvvm.Windows
{
    public abstract class WindowsAppCore
    {
        public virtual Task InitializeAsync( IActivatedEventArgs args )
        {
            return Task.CompletedTask;
        }

        public abstract Task LaunchAsync( LaunchActivatedEventArgs args );

        public virtual Task ActivateAsync( IActivatedEventArgs args )
        {
            return Task.CompletedTask;
        }

        public virtual Task ResumeAsync( IActivatedEventArgs args )
        {
            return Task.CompletedTask;
        }
    }
}