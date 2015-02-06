// Copyright (c) 2015 Solal Pirelli
// See License.txt file for more details

using System;
using System.Reflection;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Delegate that take a weak reference on its target.
    /// </summary>
    internal sealed class WeakDelegate
    {
        private const string ClosureMethodToken = "<";

        private readonly MethodInfo _method;
        private readonly WeakReference<object> _targetRef;

#pragma warning disable 0414 // Field is assigned but never used
        // If the delegate is a closure, a strong reference to the target is needed
        private readonly object _targetStrongRef;
#pragma warning restore 0414

        /// <summary>
        /// Creates a new instance of the <see cref="WeakDelegate" /> class with the specified delegate.
        /// </summary>
        /// <param name="wrapped">The delegate to be wrapped.</param>
        public WeakDelegate( Delegate wrapped )
        {
            _method = wrapped.GetMethodInfo();
            _targetRef = wrapped.Target == null ? null : new WeakReference<object>( wrapped.Target );
            _targetStrongRef = _method.Name.Contains( ClosureMethodToken ) ? wrapped.Target : null;
        }

        /// <summary>
        /// Attemps to invoke the delegate.
        /// </summary>
        /// <param name="parameters">The invocation parameters.</param>
        /// <param name="result">The result of the invocation.</param>
        /// <returns>True if the delegate is still alive; false otherwise.</returns>
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

        /// <summary>
        /// Attempts to compare the delegate with another delegate.
        /// </summary>
        /// <param name="other">The other delegate.</param>
        /// <param name="result">The result of the comparison.</param>
        /// <returns>True if the delegate is still alive; false otherwise.</returns>
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