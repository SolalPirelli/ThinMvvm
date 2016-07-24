using System;

namespace ThinMvvm.Tests.TestInfrastructure
{
    /// <summary>
    /// Custom exception class, so that exception-expecting tests can be sure it was not thrown by the code under test.
    /// </summary>
    public sealed class MyException : Exception { }
}