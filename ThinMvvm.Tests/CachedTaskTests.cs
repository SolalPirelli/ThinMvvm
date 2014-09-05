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
            long id = 10;
            var now = DateTimeOffset.Now.AddDays( 1 );
            var task = CachedTask.Create( () => Task.FromResult( 0 ), id, now );

            Assert.IsTrue( task.HasData );
            Assert.IsTrue( task.ShouldBeCached );
            Assert.AreEqual( id, task.Id );
            Assert.AreEqual( now, task.ExpirationDate );
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
            var task = CachedTask.DoNotCache( () => Task.FromResult( 0 ) );

            Assert.IsTrue( task.HasData );
            Assert.IsFalse( task.ShouldBeCached );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void DoNotCache_ErrorOnNullGetter()
        {
            CachedTask.DoNotCache<int>( null );
        }

        [TestMethod]
        public void NoNewData_Works()
        {
            var task = CachedTask.NoNewData<int>();

            Assert.IsFalse( task.HasData );
            Assert.IsFalse( task.ShouldBeCached );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentException ) )]
        public void ErrorWhenExpirationDateIsInPast()
        {
            CachedTask.Create( () => Task.FromResult( 0 ), expirationDate: DateTime.Now.AddSeconds( -1 ) );
        }

        [TestMethod]
        public void DefaultIdIsZero()
        {
            var task = CachedTask.Create( () => Task.FromResult( 0 ) );

            Assert.AreEqual( 0, task.Id );
        }

        [TestMethod]
        public void DefaultExpirationDateIsMaxValue()
        {
            var task = CachedTask.Create( () => Task.FromResult( 0 ) );

            Assert.AreEqual( DateTimeOffset.MaxValue, task.ExpirationDate );
        }
    }
}