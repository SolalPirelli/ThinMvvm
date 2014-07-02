// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThinMvvm.Internals;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Logs navigations and events.
    /// </summary>
    public abstract class Logger
    {
        private readonly INavigationService _navigationService;
        private readonly Dictionary<Type, ILogValueConverter> _converters;

        private string _currentViewModelId;


        /// <summary>
        /// Initializes a new instance of the <see cref="Logger" /> class, that will log the specified
        /// <see cref="INavigationService" />.
        /// </summary>
        protected Logger( INavigationService navigationService )
        {
            _navigationService = navigationService;
            _converters = new Dictionary<Type, ILogValueConverter>();
        }


        /// <summary>
        /// Starts the logger.
        /// </summary>
        public void Start()
        {
            _navigationService.Navigated += NavigationService_Navigated;

            Messenger.Register<CommandLoggingRequest>( req => EnableCommandLogging( req.Object ) );
            Messenger.Register<EventLogRequest>( req => LogCommand( req.ScreenId ?? _currentViewModelId, req.EventId, req.Label ) );
        }


        /// <summary>
        /// Logs an action executed on the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel ID.</param>
        /// <param name="action">The action.</param>
        protected abstract void LogAction( string viewModelId, SpecialAction action );

        /// <summary>
        /// Logs a command execution on the specified ViewModel with the specified ID and label.
        /// </summary>
        /// <param name="viewModelId">The ViewModel ID.</param>
        /// <param name="eventId">The event ID.</param>
        /// <param name="label">The label.</param>
        protected abstract void LogCommand( string viewModelId, string eventId, string label );


        /// <summary>
        /// Enables logging of commands on the specified object.
        /// </summary>
        private void EnableCommandLogging( object obj )
        {
            foreach ( var prop in GetAllProperties( obj.GetType().GetTypeInfo() ).Where( pi => IsCommand( pi.PropertyType ) ) )
            {
                var command = (CommandBase) prop.GetValue( obj );

                var actionAttr = prop.GetCustomAttribute<SpecialCommandAttribute>();
                if ( actionAttr != null )
                {
                    command.Executed += ( _, e ) => LogAction( _currentViewModelId, actionAttr.Action );
                    continue;
                }

                var idAttr = prop.GetCustomAttribute<LogIdAttribute>();
                if ( idAttr != null )
                {
                    var parameterAttr = prop.GetCustomAttribute<LogParameterAttribute>();
                    if ( parameterAttr == null )
                    {
                        command.Executed += ( _, e ) => LogCommand( _currentViewModelId, idAttr.Id, null );
                    }
                    else
                    {
                        var parameterPath = parameterAttr.ParameterPath.Split( LogParameterAttribute.PathSeparator );

                        var converterAttr = prop.GetCustomAttribute<LogValueConverterAttribute>();
                        var converterType = converterAttr == null ? typeof( IdentityLogValueConverter ) : converterAttr.ConverterType;
                        if ( !_converters.ContainsKey( converterType ) )
                        {
                            _converters.Add( converterType, (ILogValueConverter) Activator.CreateInstance( converterType ) );
                        }

                        command.Executed += ( _, e ) =>
                        {
                            object value = GetPathValue( parameterPath, obj, e.Argument );
                            string converted = _converters[converterType].Convert( value );
                            LogCommand( _currentViewModelId, idAttr.Id, converted );
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Handles navigation events.
        /// </summary>
        private void NavigationService_Navigated( object sender, NavigatedEventArgs e )
        {
            var vmLogAttr = e.ViewModel.GetType().GetTypeInfo().GetCustomAttribute<LogIdAttribute>();
            if ( vmLogAttr != null )
            {
                _currentViewModelId = vmLogAttr.Id;

                LogAction( vmLogAttr.Id, e.IsForward ? SpecialAction.ForwardsNavigation : SpecialAction.BackwardsNavigation );

                if ( e.IsForward )
                {
                    EnableCommandLogging( e.ViewModel );
                }
            }
        }


        /// <summary>
        /// Get all properties of a type info, including those declared by base types.
        /// </summary>
        private static IEnumerable<PropertyInfo> GetAllProperties( TypeInfo typeInfo )
        {
            var props = new List<PropertyInfo>();

            while ( typeInfo != null )
            {
                props.AddRange( typeInfo.DeclaredProperties );
                typeInfo = typeInfo.BaseType == null ? null : typeInfo.BaseType.GetTypeInfo();
            }

            return props;
        }

        /// <summary>
        /// Indicates whether the specified type derives from CommandBase.
        /// </summary>
        private static bool IsCommand( Type type )
        {
            return typeof( CommandBase ).GetTypeInfo().IsAssignableFrom( type.GetTypeInfo() );
        }

        /// <summary>
        /// Evaluates a path on a root, with a parameter that may be used depending on the path.
        /// </summary>
        private static object GetPathValue( string[] path, object root, object parameter )
        {
            int n = 0;
            if ( path[0] == LogParameterAttribute.ParameterName )
            {
                root = parameter;
                n++;
            }

            while ( n < path.Length )
            {
                root = GetAllProperties( root.GetType().GetTypeInfo() ).First( p => p.Name == path[n] ).GetValue( root );
                n++;
            }

            return root;
        }

        /// <summary>
        /// Implements the <see cref="ILogValueConverter" /> interface by doing a simple ToString conversion.
        /// </summary>
        private sealed class IdentityLogValueConverter : ILogValueConverter
        {
            /// <summary>
            /// Converts an object to its string representation via ToString.
            /// </summary>
            public string Convert( object value )
            {
                return value.ToString();
            }
        }
    }
}