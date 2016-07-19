using System;
using System.Collections.Generic;
using ThinMvvm.Infrastructure;

namespace ThinMvvm
{
    public sealed class ViewBinder<TViewBase> : IViewRegistry
    {
        private readonly Dictionary<Type, Type> _views;
        private readonly Dictionary<Type, Type> _viewModels;


        public ViewBinder()
        {
            _views = new Dictionary<Type, Type>();
            _viewModels = new Dictionary<Type, Type>();
        }


        public void Bind<TViewModel, TView>()
            where TViewModel : IViewModel
            where TView : TViewBase
        {
            if( _views.ContainsKey( typeof( TViewModel ) ) )
            {
                throw new ArgumentException( "The ViewModel was already bound to a View." );
            }
            if( _viewModels.ContainsKey( typeof( TView ) ) )
            {
                throw new ArgumentException( "The View was already bound to a ViewModel." );
            }

            _views.Add( typeof( TViewModel ), typeof( TView ) );
            _viewModels.Add( typeof( TView ), typeof( TViewModel ) );
        }


        Type IViewRegistry.GetViewType( Type viewModelType )
        {
            if( viewModelType == null )
            {
                throw new ArgumentNullException( nameof( viewModelType ) );
            }

            Type viewType;
            if( _views.TryGetValue( viewModelType, out viewType ) )
            {
                return viewType;
            }

            throw new InvalidOperationException( $"Unknown ViewModel type: '{viewModelType.FullName}'." );
        }

        Type IViewRegistry.GetViewModelType( Type viewType )
        {
            if( viewType == null )
            {
                throw new ArgumentNullException( nameof( viewType ) );
            }

            Type viewModelType;
            if( _viewModels.TryGetValue( viewType, out viewModelType ) )
            {
                return viewModelType;
            }

            throw new InvalidOperationException( $"Unknown View type: '{viewType.FullName}'." );
        }
    }
}