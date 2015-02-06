// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThinMvvm
{
    /// <summary>
    /// <see cref="DataViewModel{TParameter}" /> that can cache data.
    /// </summary>
    /// <typeparam name="TParameter">The type of the ViewModel's constructor parameter, or <see cref="NoParameter" /> if it does not have one.</typeparam>
    /// <typeparam name="TData">The type of the cached data.</typeparam>
    public abstract class CachedDataViewModel<TParameter, TData> : DataViewModel<TParameter>
    {
        private readonly IDataCache _cache;
        private long? _currentDataId;

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
        protected CachedDataViewModel( IDataCache cache )
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
            if ( token == null )
            {
                throw new ArgumentNullException( "token" );
            }

            var newTask = GetData( force, token );

            if ( !newTask.HasData )
            {
                return;
            }

            var cachedData = await _cache.GetAsync<TData>( GetType(), newTask.Id );

            if ( cachedData.HasData )
            {
                if ( _currentDataId != newTask.Id )
                {
                    HandleData( cachedData.Data, token );
                }
                CacheStatus = CacheStatus.UsedTemporarily;
            }
            else
            {
                CacheStatus = CacheStatus.NoData;
            }

            try
            {
                var newData = await newTask.GetDataAsync();
                _currentDataId = newTask.Id;

                if ( HandleData( newData, token ) && newTask.ShouldBeCached )
                {
                    await _cache.SetAsync( GetType(), newTask.Id, newTask.ExpirationDate, newData );
                    CacheStatus = CacheStatus.Unused;
                }
                else
                {
                    CacheStatus = CacheStatus.OptedOut;
                }
            }
            catch
            {
                if ( CacheStatus == CacheStatus.UsedTemporarily )
                {
                    CacheStatus = CacheStatus.Used;
                }
                else
                {
                    CacheStatus = CacheStatus.NoData;
                }

                throw;
            }
        }
    }
}