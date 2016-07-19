using System;
using System.Threading.Tasks;

namespace ThinMvvm.Tests.TestInfrastructure
{
    // COMPAT: Methods that are not in Profile111.
    public static class TaskEx
    {
        public static readonly Task CompletedTask = Task.FromResult( 0 );

        public static Task<T> FromException<T>( Exception exception )
        {
            var source = new TaskCompletionSource<T>();
            source.SetException( exception );
            return source.Task;
        }
    }
}