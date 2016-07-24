using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Represents a source of data.
    /// </summary>
    /// <remarks>
    /// Implementations MUST fire <see cref="INotifyPropertyChanged.PropertyChanged" /> when any property changes,
    /// and MUST fire the change event for <see cref="Status" /> after any group of properties change,
    /// to make it easy for clients to listen to any change in the source.
    /// </remarks>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public interface IDataSource : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the data loaded by the source, if any.
        /// The returned collection may implement <see cref="INotifyCollectionChanged" />.
        /// </summary>
        IReadOnlyList<IDataChunk> Data { get; }

        /// <summary>
        /// Gets the source's status.
        /// </summary>
        DataSourceStatus Status { get; }
        
        /// <summary>
        /// Gets a value indicating whether there is more data to be fetched.
        /// If this is <c>true</c>, calling <see cref="FetchMoreAsync" /> will add a chunk to <see cref="Data" />.
        /// </summary>
        bool CanFetchMore { get; }


        /// <summary>
        /// Asynchronously refreshes the data.
        /// </summary>
        /// <returns>A task that represents the refresh operation.</returns>
        Task RefreshAsync();

        /// <summary>
        /// Asynchronously fetches more data.
        /// </summary>
        /// <returns>A task that represents the fetch operation.</returns>
        Task FetchMoreAsync();
    }
}