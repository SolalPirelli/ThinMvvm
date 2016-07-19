using System;
using System.ComponentModel;

namespace ThinMvvm.Infrastructure
{
    /// <summary>
    /// Infrastructure.
    /// Contains mappings from View to ViewModel types.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Advanced )]
    public interface IViewRegistry
    {
        Type GetViewType( Type viewModelType );

        Type GetViewModelType( Type viewType );
    }
}
