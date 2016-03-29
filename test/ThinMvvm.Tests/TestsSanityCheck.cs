using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Sanity checks on the tests themselves.
    /// </summary>
    public sealed class TestsSanityCheck
    {
        [Fact]
        public void NoAsyncVoidMethods()
        {
            // Original idea by Phil Haack: http://haacked.com/archive/2014/11/11/async-void-methods/

            var asyncVoidMethods =
                typeof( TestsSanityCheck ).GetTypeInfo().Assembly.GetTypes()
                    .SelectMany( type => type.GetMethods(
                       BindingFlags.NonPublic | BindingFlags.Public |
                       BindingFlags.Instance | BindingFlags.Static |
                       BindingFlags.DeclaredOnly ) )
                    .Where( m => m.GetCustomAttributes<AsyncStateMachineAttribute>().Any() )
                    .Where( m => m.ReturnType == typeof( void ) );

            var messages =
                asyncVoidMethods
                    .Select( m => $"'{ m.DeclaringType.Name}.{m.Name}' is an async void method." )
                    .ToArray();

            Assert.False( messages.Any(),
                "Async void methods found!" + Environment.NewLine + string.Join( Environment.NewLine, messages ) );
        }

        [Fact]
        public void NoPublicNonTestMethods()
        {
            var publicNonTestMethods =
                typeof( TestsSanityCheck ).GetTypeInfo().Assembly.GetTypes()
                    .SelectMany( type => type.GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) )
                    .Where( m => !m.Attributes.HasFlag( MethodAttributes.NewSlot ) ) // exclude 'new'-ed methods
                    .Where( m => m.GetBaseDefinition() == null ) // exclude overriden methods
                    .Where( m => !m.GetCustomAttributes<FactAttribute>().Any() && !m.GetCustomAttributes<TheoryAttribute>().Any() );

            var messages =
                publicNonTestMethods
                    .Select( m => $"'{m.DeclaringType.Name}.{m.Name} is a public non-test method." )
                    .ToArray();

            Assert.False( messages.Any(),
                "Public non-test methods found!" + Environment.NewLine + string.Join( Environment.NewLine, messages ) );
        }
    }
}