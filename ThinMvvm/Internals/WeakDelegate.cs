// Copyright (c) 2015 Solal Pirelli
// See License.txt file for more details

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Delegate that does its best to only have a weak reference to its target.
    /// </summary>
    internal sealed class WeakDelegate
    {
        private readonly MethodInfo _method;
        // If the delegate isn't friendly to reflection, bail out and keep a strong reference.
        private readonly Delegate _original;
        // If the delegate is a closure, a strong reference to the target is needed, since there are no other references to it
        private readonly object _targetStrongRef;
        // Otherwise we have a normal delegate and we only keep a weak reference
        private readonly WeakReference<object> _targetRef;

        /// <summary>
        /// Creates a new instance of the <see cref="WeakDelegate" /> class with the specified delegate.
        /// </summary>
        /// <param name="wrapped">The delegate to be wrapped.</param>
        public WeakDelegate( Delegate wrapped )
        {
            _method = wrapped.GetMethodInfo();

            var targetType = wrapped.Target == null ? null : wrapped.Target.GetType().GetTypeInfo();
            _original =
                // Open delegate
                ( wrapped.Target == null && !_method.IsStatic )
                // Private type, possibly untrusted code, better bail out
             || ( targetType != null && targetType.IsNotPublic && ( !targetType.IsNested || targetType.IsNestedPublic ) ) ?
                wrapped : null;

            _targetStrongRef =
                _original == null && _method.DeclaringType.GetTypeInfo().GetCustomAttribute<CompilerGeneratedAttribute>() == null ?
                null : wrapped.Target;

            _targetRef =
                _targetStrongRef == null && wrapped.Target != null ?
                new WeakReference<object>( wrapped.Target ) : null;
        }

        /// <summary>
        /// Attemps to invoke the delegate.
        /// </summary>
        /// <param name="parameters">The invocation parameters.</param>
        /// <param name="result">The result of the invocation.</param>
        /// <returns>True if the delegate is still alive; false otherwise.</returns>
        public bool TryInvoke( object[] parameters, out object result )
        {
            object target;

            if ( _original != null )
            {
                result = _original.DynamicInvoke( parameters );
                return true;
            }

            if ( _targetStrongRef != null )
            {
                result = _method.Invoke( _targetStrongRef, parameters );
                return true;
            }

            if ( _targetRef == null )
            {
                result = _method.Invoke( null, parameters );
                return true;
            }

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

            if ( _original != null )
            {
                result = _original == other;
                return true;
            }

            if ( _targetStrongRef != null )
            {
                result = _method == other.GetMethodInfo()
                      && _targetStrongRef == other.Target;
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