// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm
{
    /// <summary>
    /// DataViewModel that can cache data.
    /// </summary>
    /// <typeparam name="TParameter">The type of the ViewModel's constructor parameter, or <see cref="NoParameter" /> if it does not have one.</typeparam>
    /// <typeparam name="TData">The type of the cached data.</typeparam>
    public abstract class CachedDataViewModel<TParameter, TData> : DataViewModel<TParameter>
    {
        private const long DefaultId = 0;
        private static readonly DateTime DefaultExpirationDate = DateTime.MaxValue;

        private readonly ICache _cache;

        private CacheStatus _cacheStatus;

        /// <summary>
        /// Gets the cache status of the ViewModel.
        /// </summary>
        public CacheStatus CacheStatus
        {
            get { return _cacheStatus; }
            private set { SetProperty( ref _cacheStatus, value ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CachedDataViewModel{TParameter, TData}" /> class with the specified cache.
        /// </summary>
        /// <param name="cache">The cache used by the ViewModel.</param>
        protected CachedDataViewModel( ICache cache )
        {
            _cache = cache;
        }


        /// <summary>
        /// Gets refreshed data that can be cached, or explicit instructions to not cache it.
        /// </summary>
        /// <param name="force">A value indicating whether to force a refresh or not.</param>
        /// <param name="token">The cancellation token for the refresh request.</param>
        /// <returns>A <see cref="CachedTask" /> instance indicating what to get and whether to cache it.</returns>
        protected abstract CachedTask<TData> GetData( bool force, CancellationToken token );

        /// <summary>
        /// Handles data, either cached or live.
        /// </summary>
        /// <param name="data">The data, which can be cached or live.</param>
        /// <param name="token">The cancellation token for the refresh request.</param>
        /// <returns>True if the data should be cached, false otherwise.</returns>
        protected abstract bool HandleData( TData data, CancellationToken token );


        /// <summary>
        /// Asynchronously refreshes the data using cached data both as a placeholder and as a fallback.
        /// </summary>
        /// <param name="force">A value indicating whether to force a refresh or not.</param>
        /// <param name="token">The cancellation token for the refresh request.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override sealed async Task RefreshAsync( bool force, CancellationToken token )
        {
            var cachedData = GetData( force, token );

            TData data = default( TData );
            if ( _cache.TryGet( this.GetType(), cachedData.Id ?? DefaultId, out data ) )
            {
                CacheStatus = CacheStatus.UsedTemporarily;
                HandleData( data, token );
            }
            else if ( cachedData.HasNewData )
            {
                CacheStatus = CacheStatus.Loading;
            }

            try
            {
                if ( cachedData.HasNewData )
                {
                    data = await cachedData.GetDataAsync();
                    if ( HandleData( data, token ) && cachedData.ShouldBeCached )
                    {
                        if ( cachedData.HasNewData )
                        {
                            _cache.Set( this.GetType(), cachedData.Id ?? DefaultId, cachedData.ExpirationDate ?? DefaultExpirationDate, data );
                        }

                        CacheStatus = CacheStatus.Unused;
                    }
                    else
                    {
                        CacheStatus = CacheStatus.OptedOut;
                    }
                }
            }
            catch ( Exception e )
            {
                if ( DataViewModelOptions.IsNetworkException( e ) )
                {
                    if ( CacheStatus == CacheStatus.UsedTemporarily )
                    {
                        CacheStatus = CacheStatus.Used;
                    }
                }

                if ( CacheStatus != CacheStatus.Used )
                {
                    CacheStatus = CacheStatus.NoCache;
                    throw;
                }
            }
        }
    }
}