// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading.Tasks;

namespace ThinMvvm
{
    /// <summary>
    /// Holds an asynchronous operation whose result can be cached.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    public sealed class CachedTask<T>
    {
        private readonly Func<Task<T>> _getter;

        /// <summary>
        /// Gets the task's ID, if it has one.
        /// </summary>
        public long? Id { get; private set; }

        /// <summary>
        /// Gets the task's result's expiration date, if it has one.
        /// </summary>
        public DateTime? ExpirationDate { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the task has any new data.
        /// </summary>
        public bool HasNewData { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the task's result should be cached.
        /// </summary>
        public bool ShouldBeCached { get; private set; }


        /// <summary>
        /// Creates a new instance of the CachedTask class with the specified parameters.
        /// </summary>
        internal CachedTask( Func<Task<T>> getter, long? id, DateTime? expirationDate, bool hasNewData, bool shouldBeCached )
        {
            _getter = getter;
            Id = id;
            ExpirationDate = expirationDate;
            HasNewData = hasNewData;
            ShouldBeCached = shouldBeCached;
        }


        /// <summary>
        /// Asynchronously gets the data held by the task.
        /// </summary>
        /// <returns>The data.</returns>
        public Task<T> GetDataAsync()
        {
            return _getter();
        }
    }

    /// <summary>
    /// Helper class to create <see cref="CachedTask{T}" /> instances.
    /// </summary>
    public static class CachedTask
    {
        /// <summary>
        /// Creates a task with a result that will be cached, an optional ID and an optional expiration date.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="getter">The asynchronous result getter.</param>
        /// <param name="id">The ID, if any.</param>
        /// <param name="expirationDate">The expiration date, if any.</param>
        /// <returns>A task with the specified parameters.</returns>
        public static CachedTask<T> Create<T>( Func<Task<T>> getter, long? id = null, DateTime? expirationDate = null )
        {
            return new CachedTask<T>( getter, id, expirationDate, true, true );
        }

        /// <summary>
        /// Creates a task whose result will not be cached.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="getter">The asynchronous result getter.</param>
        /// <returns>A task with the specified result getter.</returns>
        public static CachedTask<T> DoNotCache<T>( Func<Task<T>> getter )
        {
            return new CachedTask<T>( getter, null, null, true, false );
        }

        /// <summary>
        /// Creates a task without new data.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <returns>A task without new data.</returns>
        public static CachedTask<T> NoNewData<T>()
        {
            return new CachedTask<T>( null, null, null, false, false );
        }
    }
}