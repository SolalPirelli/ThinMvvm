using System;
using System.Reflection;

namespace ThinMvvm.Internals
{
    internal sealed class WeakDelegate
    {
        private const string ClosureMethodToken = "<";

        private readonly MethodInfo _method;
#pragma warning disable 0414 // Field is assigned but never used
        // If the delegate is a closure, a strong reference to the target is needed
        private readonly object _targetStrongRef;
#pragma warning restore 0414
        private readonly WeakReference<object> _targetRef;

        public WeakDelegate( Delegate wrapped )
        {
            _method = wrapped.GetMethodInfo();
            _targetStrongRef = _method.Name.Contains( ClosureMethodToken ) ? wrapped.Target : null;
            _targetRef = wrapped.Target == null ? null : new WeakReference<object>( wrapped.Target );
        }

        public bool TryInvoke( object[] parameters, out object result )
        {
            if ( _targetRef == null )
            {
                result = _method.Invoke( null, parameters );
                return true;
            }

            object target;
            if ( _targetRef.TryGetTarget( out target ) )
            {
                result = _method.Invoke( target, parameters );
                return true;
            }

            result = null;
            return false;
        }

        public bool TryEquals( Delegate other, out bool result )
        {
            if ( other == null )
            {
                result = false;
                return true;
            }

            if ( _targetRef == null )
            {
                result = _method == other.GetMethodInfo();
                return true;
            }

            object target;
            if ( _targetRef.TryGetTarget( out target ) )
            {
                result = _method == other.GetMethodInfo()
                      && target == other.Target;
                return true;
            }

            result = false;
            return false;
        }
    }
}