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
    /// Visits expressions and builds sequences of (object, property name) tuples for observable properties found in the expressions.
    /// </summary>
    internal sealed class ObservablePropertyVisitor : ExpressionVisitor
    {
        private List<Tuple<INotifyPropertyChanged, string>> _propertyAccesses;

        private ObservablePropertyVisitor( Expression expr )
        {
            _propertyAccesses = new List<Tuple<INotifyPropertyChanged, string>>();
            Visit( expr );
        }

        public static IEnumerable<Tuple<INotifyPropertyChanged, string>> GetObservablePropertyAccesses( Expression expr )
        {
            return new ObservablePropertyVisitor( expr )._propertyAccesses;
        }

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
            var field = (FieldInfo) memberExpr.Member;
            return Tuple.Create( field.GetValue( memberConstExpr.Value ), propertyExpr.Member.Name );
        }
    }
}