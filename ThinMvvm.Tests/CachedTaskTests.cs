// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class CachedTaskTests
    {
        [TestMethod]
        public void Create_Works()
        {
            CachedTask.Create( () => Task.FromResult( 0 ) );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Create_ErrorOnNullGetter()
        {
            CachedTask.Create<int>( null );
        }

        [TestMethod]
        public void DoNotCache_Works()
        {
            CachedTask.DoNotCache( () => Task.FromResult( 0 ) );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void DoNotCache_ErrorOnNullGetter()
        {
            CachedTask.DoNotCache<int>( null );
        }
    }
}