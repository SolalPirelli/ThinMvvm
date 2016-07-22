using System.Threading;

namespace ThinMvvm.Data.Infrastructure
{
    public sealed class CancellationTokenCreator
    {
        private readonly object _lock;
        private CancellationTokenSource _cancellationTokenSource;


        public CancellationTokenCreator()
        {
            _lock = new object();
        }


        public CancellationToken CreateAndCancelPrevious()
        {
            lock( _lock )
            {
                if( _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested )
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                _cancellationTokenSource = new CancellationTokenSource();

                return _cancellationTokenSource.Token;
            }
        }
    }
}