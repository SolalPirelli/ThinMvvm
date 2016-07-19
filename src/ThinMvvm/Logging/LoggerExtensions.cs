using System;
using ThinMvvm.Infrastructure;

namespace ThinMvvm.Logging
{
    /// <summary>
    /// Extensions to <see cref="ILogger" /> to facilitate common scenarios.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Registers the specified ViewModel for navigation logging.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="viewModel">The ViewModel.</param>
        /// <param name="id">The ViewModel's log ID.</param>
        /// <returns>An object to perform other log registrations for the same ViewModel.</returns>
        public static ViewModelRegistration Register( this ILogger logger, IViewModel viewModel, string id )
        {
            if( logger == null )
            {
                throw new ArgumentNullException( nameof( logger ) );
            }
            if( viewModel == null )
            {
                throw new ArgumentNullException( nameof( viewModel ) );
            }
            if( id == null )
            {
                throw new ArgumentNullException( nameof( id ) );
            }

            viewModel.NavigatedTo += ( _, __ ) => logger.LogNavigation( id, true );
            viewModel.NavigatedFrom += ( _, __ ) => logger.LogNavigation( id, false );

            return new ViewModelRegistration( logger, id );
        }


        /// <summary>
        /// Infrastructure.
        /// Contains methods to register logging operations on a ViewModel.
        /// </summary>
        public sealed class ViewModelRegistration
        {
            private readonly ILogger _logger;
            private readonly string _viewModelId;

            /// <summary>
            /// Initializes a new instance of the <see cref="ViewModelRegistration" /> class.
            /// </summary>
            /// <param name="logger">The logger to use.</param>
            /// <param name="viewModelId">The ViewModel's ID.</param>
            internal ViewModelRegistration( ILogger logger, string viewModelId )
            {
                _logger = logger;
                _viewModelId = viewModelId;
            }


            /// <summary>
            /// Registers the specified command for event logging.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="id">The command's log ID.</param>
            /// <param name="labelCreator">A function to specify the label for each execution, if necessary</param>
            /// <returns>The current <see cref="ViewModelRegistration" />, for method call chaining.</returns>
            public ViewModelRegistration WithCommand( Command command, string id, Func<string> labelCreator = null )
            {
                if( command == null )
                {
                    throw new ArgumentNullException( nameof( command ) );
                }
                if( id == null )
                {
                    throw new ArgumentNullException( nameof( id ) );
                }

                command.Executed += ( _, __ ) => _logger.LogEvent( _viewModelId, id, labelCreator?.Invoke() );
                return this;
            }

            /// <summary>
            /// Registers the specified command for event logging.
            /// </summary>
            /// <typeparam name="T">The command's parameter type.</typeparam>
            /// <param name="command">The command.</param>
            /// <param name="id">The command's log ID.</param>
            /// <param name="labelCreator">A function to specify the label for each execution, if necessary</param>
            /// <returns>The current <see cref="ViewModelRegistration" />, for method call chaining.</returns>
            public ViewModelRegistration WithCommand<T>( Command<T> command, string id, Func<T, string> labelCreator = null )
            {
                if( command == null )
                {
                    throw new ArgumentNullException( nameof( command ) );
                }
                if( id == null )
                {
                    throw new ArgumentNullException( nameof( id ) );
                }

                command.Executed += ( _, e ) => _logger.LogEvent( _viewModelId, id, labelCreator?.Invoke( (T) e.Argument ) );
                return this;
            }

            /// <summary>
            /// Registers the specified command for event logging.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="id">The command's log ID.</param>
            /// <param name="labelCreator">A function to specify the label for each execution, if necessary</param>
            /// <returns>The current <see cref="ViewModelRegistration" />, for method call chaining.</returns>
            public ViewModelRegistration WithCommand( AsyncCommand command, string id, Func<string> labelCreator = null )
            {
                if( command == null )
                {
                    throw new ArgumentNullException( nameof( command ) );
                }
                if( id == null )
                {
                    throw new ArgumentNullException( nameof( id ) );
                }

                command.Executed += ( _, __ ) => _logger.LogEvent( _viewModelId, id, labelCreator?.Invoke() );
                return this;
            }

            /// <summary>
            /// Registers the specified command for event logging.
            /// </summary>
            /// <typeparam name="T">The command's parameter type.</typeparam>
            /// <param name="command">The command.</param>
            /// <param name="id">The command's log ID.</param>
            /// <param name="labelCreator">A function to specify the label for each execution, if necessary</param>
            /// <returns>The current <see cref="ViewModelRegistration" />, for method call chaining.</returns>
            public ViewModelRegistration WithCommand<T>( AsyncCommand<T> command, string id, Func<T, string> labelCreator = null )
            {
                if( command == null )
                {
                    throw new ArgumentNullException( nameof( command ) );
                }
                if( id == null )
                {
                    throw new ArgumentNullException( nameof( id ) );
                }

                command.Executed += ( _, e ) => _logger.LogEvent( _viewModelId, id, labelCreator?.Invoke( (T) e.Argument ) );
                return this;
            }
        }
    }
}