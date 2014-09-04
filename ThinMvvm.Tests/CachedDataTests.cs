// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class CachedDataTests
    {
        [TestMethod]
        public void WithNoData_NotHasData()
        {
            var data = new CachedData<int>();

            Assert.IsFalse( data.HasData );
        }

        [TestMethod]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void WithNoData_ThrowsWhenDataIsAccessed()
        {
            var data = new CachedData<int>();

            int n = data.Data;
        }

        [TestMethod]
        public void WithData_HasData()
        {
            var data = new CachedData<int>( 1 );

            Assert.IsTrue( data.HasData );
        }

        [TestMethod]
        public void WithData_DataIsCorrect()
        {
            var data = new CachedData<int>( 1 );

            Assert.AreEqual( 1, data.Data );
        }
    }
}