// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace ThinMvvm.Internals
{
    /// <summary>
    /// Visits expressions and builds sequences of (object, property name) tuples 
    /// for observable properties found in the expressions.
    /// </summary>
    internal sealed class ObservablePropertyVisitor : ExpressionVisitor
    {
        private readonly List<Tuple<INotifyPropertyChanged, string>> _propertyAccesses = new List<Tuple<INotifyPropertyChanged, string>>();

        /// <summary>
        /// Gets (object, property name) tuples for all accesses of observable properties in the specified expression.
        /// </summary>
        public static IEnumerable<Tuple<INotifyPropertyChanged, string>> GetObservablePropertyAccesses( Expression expr )
        {
            var visitor = new ObservablePropertyVisitor();
            visitor.Visit( expr );
            return visitor._propertyAccesses;
        }


        /// <summary>
        /// Visits either a field or a property.
        /// </summary>
        protected override Expression VisitMember( MemberExpression node )
        {
            if ( node.Member is PropertyInfo )
            {
                var ownerAndName = GetPropertyOwnerAndName( node );
                var ownerNotif = ownerAndName.Item1 as INotifyPropertyChanged;
                if ( ownerNotif != null )
                {
                    _propertyAccesses.Add( Tuple.Create( ownerNotif, ownerAndName.Item2 ) );
                }
            }

            return base.VisitMember( node );
        }


        /// <summary>
        /// Extracts the owner and name of the property represented by the specified expression.
        /// </summary>
        private static Tuple<object, string> GetPropertyOwnerAndName( MemberExpression propertyExpr )
        {
            var constExpr = propertyExpr.Expression as ConstantExpression;
            if ( constExpr != null )
            {
                return Tuple.Create( constExpr.Value, propertyExpr.Member.Name );
            }

            // Magic to get the owner/name of a property access not on 'this'
            var memberExpr = (MemberExpression) propertyExpr.Expression;
            var memberConstExpr = (ConstantExpression) memberExpr.Expression;
            object value;
            var field = memberExpr.Member as FieldInfo;
            if ( field != null )
            {
                value = field.GetValue( memberConstExpr.Value );
            }
            else
            {
                var prop = memberExpr.Member as PropertyInfo;
                value = prop.GetValue( memberConstExpr.Value );
            }

            return Tuple.Create( value, propertyExpr.Member.Name );
        }
    }
}