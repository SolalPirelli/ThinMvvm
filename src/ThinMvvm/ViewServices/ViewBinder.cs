using System;
using System.Collections.Generic;
using System.ComponentModel;
using ThinMvvm.Infrastructure;
using ThinMvvm.ViewServices.Infrastructure;

namespace ThinMvvm.ViewServices
{
    /// <summary>
    /// Binds Views to ViewModels.
    /// </summary>
    /// <typeparam name="TViewBase">The base type for Views.</typeparam>
    public sealed class ViewBinder<TViewBase>
    {
        private readonly Dictionary<Type, Type> _viewModelsToViews;
        private readonly Dictionary<Type, Type> _viewsToViewModels;


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBinder{TViewBase}" /> class.
        /// </summary>
        public ViewBinder()
        {
            _viewModelsToViews = new Dictionary<Type, Type>();
            _viewsToViewModels = new Dictionary<Type, Type>();
        }


        /// <summary>
        /// Binds the specified ViewModel and View types.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        /// <typeparam name="TView">The View type.</typeparam>
        public void Bind<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : TViewBase
        {
            if( _viewModelsToViews.ContainsKey( typeof( TViewModel ) ) )
            {
                throw new InvalidOperationException( "The ViewModel was already bound to a View." );
            }
            if( _viewsToViewModels.ContainsKey( typeof( TView ) ) )
            {
                throw new InvalidOperationException( "The View was already bound to a ViewModel." );
            }

            _viewModelsToViews.Add( typeof( TViewModel ), typeof( TView ) );
            _viewsToViewModels.Add( typeof( TView ), typeof( TViewModel ) );
        }


        /// <summary>
        /// Infrastructure.
        /// Builds a <see cref="ViewRegistry" /> using the bindings defined with this binder.
        /// </summary>
        /// <returns></returns>
        [EditorBrowsable( EditorBrowsableState.Advanced )]
        public ViewRegistry BuildRegistry()
        {
            return new ViewRegistry( _viewModelsToViews, _viewsToViewModels );
        }
    }
}