using System.ComponentModel;
using System.Threading;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Contains a single <see cref="CancellationToken" /> at a time, and cancels the previous one each time a new one is requested.
    /// 
    /// This class is thread safe.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class CancellationTokenHolder
    {
        private readonly object _lock;
        private CancellationTokenSource _cancellationTokenSource;


        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTokenHolder" /> class.
        /// </summary>
        public CancellationTokenHolder()
        {
            _lock = new object();
        }


        /// <summary>
        /// Atomically creates a new <see cref="CancellationToken" /> and cancels the previous one.
        /// </summary>
        /// <returns>The created <see cref="CancellationToken" />.</returns>
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