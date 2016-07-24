using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ThinMvvm.ViewServices.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Contains bidirectional bindings from View types to ViewModel types.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public sealed class ViewRegistry
    {
        private readonly Dictionary<Type, Type> _viewModelsToViews;
        private readonly Dictionary<Type, Type> _viewsToViewModels;


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRegistry" /> class with the specified bindings.
        /// </summary>
        /// <param name="viewModelsToViews">The ViewModel to View bindings.</param>
        /// <param name="viewsToViewModels">The View to ViewModel bindings.</param>
        public ViewRegistry( Dictionary<Type, Type> viewModelsToViews, Dictionary<Type, Type> viewsToViewModels )
        {
            _viewsToViewModels = new Dictionary<Type, Type>( viewsToViewModels );
            _viewModelsToViews = new Dictionary<Type, Type>( viewModelsToViews );
        }


        /// <summary>
        /// Gets the View type associated with the specified ViewModel type.
        /// </summary>
        /// <param name="viewModelType">The ViewModel type.</param>
        /// <returns>The associated View type.</returns>
        public Type GetViewType( Type viewModelType )
        {
            if( viewModelType == null )
            {
                throw new ArgumentNullException( nameof( viewModelType ) );
            }

            Type viewType;
            if( _viewModelsToViews.TryGetValue( viewModelType, out viewType ) )
            {
                return viewType;
            }

            throw new InvalidOperationException( $"Unknown ViewModel type: '{viewModelType.FullName}'." );
        }

        /// <summary>
        /// Gets the ViewModel type associated with the specified View type.
        /// </summary>
        /// <param name="viewType">The View type.</param>
        /// <returns>The associated ViewModel type.</returns>
        public Type GetViewModelType( Type viewType )
        {
            if( viewType == null )
            {
                throw new ArgumentNullException( nameof( viewType ) );
            }

            Type viewModelType;
            if( _viewsToViewModels.TryGetValue( viewType, out viewModelType ) )
            {
                return viewModelType;
            }

            throw new InvalidOperationException( $"Unknown View type: '{viewType.FullName}'." );
        }
    }
}
