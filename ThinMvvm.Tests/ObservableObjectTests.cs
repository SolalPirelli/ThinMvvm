// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public class ObservableObjectTests
    {
        private sealed class TestObservableObject : ObservableObject
        {
            public int _value;

            public int Value
            {
                get { return _value; }
                set { SetProperty( ref _value, value ); }
            }
        }

        private sealed class TestSynchronizationContext : SynchronizationContext
        {
            public int SendCount;

            public override void Send( SendOrPostCallback d, object state )
            {
                base.Send( d, state );
                SendCount++;
            }

            public override void Post( SendOrPostCallback d, object state )
            {
                throw new InvalidOperationException( "Post(SendOrPostCallback, object) should not be used." );
            }
        }

        [TestMethod]
        public void SetPropertySetsTheField()
        {
            var obj = new TestObservableObject();

            obj.Value = 42;

            Assert.AreEqual( 42, obj._value, "SetProperty() should set the field to the correct value." );
        }

        [TestMethod]
        public void SetPropertyDoesNotFirePropertyChangedIfThereWasNoChange()
        {
            var obj = new TestObservableObject();

            obj.Value = 0;
            int counter = 0;
            obj.PropertyChanged += ( s, e ) => counter++;

            obj.Value = 0;

            Assert.AreEqual( 0, counter, "SetProperty() should not fire PropertyChanged when no change occurs." );
        }

        [TestMethod]
        public void SetPropertyFiresPropertyChangedWhenNeeded()
        {
            var obj = new TestObservableObject();

            obj.Value = 0;
            int counter = 0;
            obj.PropertyChanged += ( s, e ) => counter++;

            obj.Value = 42;

            Assert.AreEqual( 1, counter, "SetProperty() should fire PropertyChanged when a change occurs." );
        }

        [TestMethod]
        public void SetPropertyFiresChangesOnCurrentSynchronizationContext()
        {
            var myContext = new TestSynchronizationContext();
            var oldContext = SynchronizationContext.Current;

            try
            {
                SynchronizationContext.SetSynchronizationContext( myContext );

                var obj = new TestObservableObject();

                obj.PropertyChanged += ( s, e ) => { };
                obj.Value = 42;

                Assert.AreEqual( 1, myContext.SendCount );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext( oldContext );
            }
        }
    }
}