// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Linq.Expressions;
using System.Reflection;

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
        /// <typeparam name="TObj">The type of the object the property belongs to.</typeparam>
        /// <typeparam name="TProp">The type of the property.</typeparam>
        /// <param name="expr">The expression.</param>
        public static string GetPropertyName<TObj, TProp>( Expression<Func<TObj, TProp>> expr )
        {
            var memberExpr = expr.Body as MemberExpression;
            if ( memberExpr == null || !( memberExpr.Member is PropertyInfo ) )
            {
                throw new ArgumentException( "Invalid expression; it must return a property." );
            }

            return memberExpr.Member.Name;
        }
    }
}