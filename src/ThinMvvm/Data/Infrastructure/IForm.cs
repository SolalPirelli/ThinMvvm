using System;
using System.ComponentModel;

namespace ThinMvvm.Data.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Represents a form that an user can fill.
    /// </summary>
    /// <remarks>
    /// Implementations must fire <see cref="INotifyPropertyChanged.PropertyChanged" /> when any property changes,
    /// and must fire the change event for <see cref="Status" /> after any group of properties change,
    /// to make it easy for clients to listen to any change in the form.
    /// </remarks>
    public interface IForm : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the form's input.
        /// </summary>
        object Input { get; }

        /// <summary>
        /// Gets the form's status.
        /// </summary>
        FormStatus Status { get; }

        /// <summary>
        /// Gets the error that occurred during the last operation, if any.
        /// </summary>
        Exception Error { get; }
    }
}