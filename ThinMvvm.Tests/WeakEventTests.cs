using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class WeakEventTests
    {
        public sealed class IntEventArgs : EventArgs
        {
            public int Value { get; private set; }

            public IntEventArgs( int value )
            {
                Value = value;
            }
        }

        public sealed class Source
        {
            private WeakEvent _event = new WeakEvent();
            public event EventHandler<IntEventArgs> Event
            {
                add { _event.Add( value ); }
                remove { _event.Remove( value ); }
            }

            public void RaiseEvent( int value )
            {
                _event.Raise( this, new IntEventArgs( value ) );
            }
        }

        public sealed class Sink
        {
            public int Counter { get; private set; }

            public void Receive( object sender, IntEventArgs e )
            {
                Counter += e.Value;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void AddNull()
        {
            var source = new Source();
            source.Event += null;
        }

        [TestMethod]
        public void RaiseOnLiveHandler()
        {
            var source = new Source();
            var sink = new Sink();
            source.Event += sink.Receive;

            source.RaiseEvent( 42 );

            Assert.AreEqual( 42, sink.Counter );

            GC.KeepAlive( sink );
        }

        [TestMethod]
        public void RaiseOnDeadHandler()
        {
            var source = new Source();
            var sink = new Sink();
            var sinkRef = new WeakReference( sink );
            source.Event += sink.Receive;

            GC.Collect();

            source.RaiseEvent( 42 );

            Assert.IsFalse( sinkRef.IsAlive );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void RemoveNull()
        {
            var source = new Source();
            source.Event -= null;
        }

        [TestMethod]
        public void RemoveLiveHandler()
        {
            var source = new Source();
            var sink = new Sink();

            source.Event += sink.Receive;
            source.Event -= sink.Receive;
            source.RaiseEvent( 42 );

            Assert.AreEqual( 0, sink.Counter );

            GC.KeepAlive( sink );
        }
    }
}