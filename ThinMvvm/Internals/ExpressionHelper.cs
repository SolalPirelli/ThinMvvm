// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Linq.Expressions;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Internal utility class for expressions.
    /// </summary>
    internal static class ExpressionHelper
    {
        /// <summary>
        /// Gets the name of the property in a property expression.
        /// </summary>
        public static string GetPropertyName<TObj, TProp>( Expression<Func<TObj, TProp>> expr )
        {
            if ( !( expr.Body is MemberExpression ) )
            {
                throw new ArgumentException( "Invalid expression; it must return a property." );
            }
            return ( (MemberExpression) expr.Body ).Member.Name;
        }
    }
}