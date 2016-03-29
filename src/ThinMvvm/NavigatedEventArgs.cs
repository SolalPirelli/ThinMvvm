using System;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.Navigated" /> event.
    /// </summary>
    public sealed class NavigatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the navigation kind.
        /// </summary>
        public NavigationKind Kind { get; }

        /// <summary>
        /// Gets the navigation target.
        /// </summary>
        public IViewModel Target { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatedEventArgs" /> class with the specified values.
        /// </summary>
        /// <param name="kind">The navigation kind.</param>
        /// <param name="target">The navigation target.</param>
        public NavigatedEventArgs( NavigationKind kind, IViewModel target )
        {
            if( !Enum.IsDefined( typeof( NavigationKind ), kind ) )
            {
                throw new ArgumentException( "Invalid enum value.", nameof( kind ) );
            }
            if( target == null )
            {
                throw new ArgumentNullException( nameof( target ) );
            }

            Kind = kind;
            Target = target;
        }
    }
}