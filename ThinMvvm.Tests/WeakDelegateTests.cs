// Copyright (c) Solal Pirelli 2015
// See License.txt file for more details

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThinMvvm.Internals;

// N.B.: Using the non-generic WeakReference is generally bad because IsAlive is a race condition,
//       but IsAlive is exactly what we want here, so it's fine.

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class WeakDelegateTests
    {
        private sealed class Static
        {
            public static int GetOne()
            {
                return 1;
            }

            public static int GetTwo()
            {
                return 2;
            }
        }

        private sealed class Instance
        {
            public int GetOne()
            {
                return 1;
            }

            public int GetTwo()
            {
                return 2;
            }
        }

        [TestMethod]
        public void TryInvokeStaticDelegate()
        {
            var weakGetOne = new WeakDelegate( new Func<int>( Static.GetOne ) );
            object result;

            GC.Collect();

            Assert.IsTrue( weakGetOne.TryInvoke( null, out result ) );
            Assert.AreEqual( 1, result );
        }

        [TestMethod]
        public void TryInvokeClosure()
        {
            int value = 0;
            var weakGetOne = new WeakDelegate( new Func<int>( () => value ) );
            value = 1;
            object result;

            GC.Collect();

            Assert.IsTrue( weakGetOne.TryInvoke( null, out result ) );
            Assert.AreEqual( 1, result );
        }

        [TestMethod]
        public void TryInvokeAliveInstanceDelegate()
        {
            var instance = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            object result;

            Assert.IsTrue( weakGetOne.TryInvoke( null, out result ) );
            Assert.AreEqual( 1, result );

            GC.KeepAlive( instance );
        }

        [TestMethod]
        public void TryInvokeDeadInstanceDelegate()
        {
            var instance = new Instance();
            var instanceRef = new WeakReference( instance );
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            object ignored;

            GC.Collect();

            Assert.IsFalse( weakGetOne.TryInvoke( null, out ignored ) );
            Assert.IsFalse( instanceRef.IsAlive );
        }

        [TestMethod]
        public void TryEqualsStaticDelegateWithSameDelegate()
        {
            var weakGetOne = new WeakDelegate( new Func<int>( Static.GetOne ) );
            bool result;

            GC.Collect();

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( Static.GetOne ), out result ) );
            Assert.IsTrue( result );
        }

        [TestMethod]
        public void TryEqualsStaticDelegateWithOtherDelegate()
        {
            var weakGetOne = new WeakDelegate( new Func<int>( Static.GetOne ) );
            bool result;

            GC.Collect();

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( Static.GetTwo ), out result ) );
            Assert.IsFalse( result );
        }

        [TestMethod]
        public void TryEqualsStaticDelegateWithInstanceDelegate()
        {
            var instance = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( Static.GetOne ) );
            bool result;

            GC.Collect();

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( instance.GetOne ), out result ) );
            Assert.IsFalse( result );
        }

        [TestMethod]
        public void TryEqualsAliveInstanceDelegateWithSameDelegateAndTarget()
        {
            var instance = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            bool result;

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( instance.GetOne ), out result ) );
            Assert.IsTrue( result );

            GC.KeepAlive( instance );
        }

        [TestMethod]
        public void TryEqualsAliveInstanceDelegateWithSameDelegateButDifferentTarget()
        {
            var instance1 = new Instance();
            var instance2 = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( instance1.GetOne ) );
            bool result;

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( instance2.GetOne ), out result ) );
            Assert.IsFalse( result );

            GC.KeepAlive( instance1 );
        }

        [TestMethod]
        public void TryEqualsAliveInstanceDelegateWithSameTargetButDifferentDelegate()
        {
            var instance = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            bool result;

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( instance.GetTwo ), out result ) );
            Assert.IsFalse( result );

            GC.KeepAlive( instance );
        }

        [TestMethod]
        public void TryEqualsAliveInstanceDelegateWithStaticDelegate()
        {
            var instance = new Instance();
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            bool result;

            Assert.IsTrue( weakGetOne.TryEquals( new Func<int>( Static.GetOne ), out result ) );
            Assert.IsFalse( result );

            GC.KeepAlive( instance );
        }

        [TestMethod]
        public void TryEqualsDeadInstanceDelegate()
        {
            var instance = new Instance();
            var instanceRef = new WeakReference( instance );
            var weakGetOne = new WeakDelegate( new Func<int>( instance.GetOne ) );
            bool result;

            GC.Collect();

            Assert.IsFalse( weakGetOne.TryEquals( new Func<int>( Static.GetOne ), out result ) );
            Assert.IsFalse( result );
            Assert.IsFalse( instanceRef.IsAlive );
        }
    }
}