using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class ObservableObjectTests
    {
        private sealed class ObservableInt : ObservableObject
        {
            private int _value;

            public int Value
            {
                set { Set( ref _value, value ); }
            }

            public new void OnPropertyChanged( string propertyName )
            {
                base.OnPropertyChanged( propertyName );
            }

            public void SetWithNullName()
            {
                Set( ref _value, 1, null );
            }
        }

        [Fact]
        public void SetFiresEventWhenValueChanges()
        {
            var obj = new ObservableInt();
            bool fired = false;
            obj.PropertyChanged += ( _, __ ) => fired = true;

            obj.Value = 1;

            Assert.True( fired );
        }

        [Fact]
        public void SetFiresEventManyTimesWhenValueChangesManyTimes()
        {
            var obj = new ObservableInt();
            int counter = 0;
            obj.PropertyChanged += ( _, __ ) => counter++;

            for( int n = 10; n < 20; n++ )
            {
                obj.Value = n;
            }

            Assert.Equal( 10, counter );
        }

        [Fact]
        public void SetDoesNotFireEventWhenValueDoesNotChange()
        {
            var obj = new ObservableInt();
            bool fired = false;
            obj.PropertyChanged += ( _, __ ) => fired = true;

            obj.Value = 0;

            Assert.False( fired );
        }

        [Fact]
        public void SetFiresEventWithCorrectName()
        {
            var obj = new ObservableInt();
            string name = null;
            obj.PropertyChanged += ( _, e ) => name = e.PropertyName;

            obj.Value = 1;

            Assert.Equal( "Value", name );
        }

        [Fact]
        public void OnPropertyChangedFiresEvent()
        {
            var obj = new ObservableInt();
            string name = null;
            obj.PropertyChanged += ( _, e ) => name = e.PropertyName;

            obj.OnPropertyChanged( "ABC123" );

            Assert.Equal( "ABC123", name );
        }

        [Fact]
        public void OnPropertyChangedDoesNotCrashWithoutListeners()
        {
            var obj = new ObservableInt();

            obj.OnPropertyChanged( "ABC" );
        }

        [Fact]
        public void SetThrowsWithNullName()
        {
            var obj = new ObservableInt();

            Assert.Throws<ArgumentNullException>( () => obj.SetWithNullName() );
        }


        [DataContract]
        private sealed class SerializableInt : ObservableObject
        {
            private int _value;

            [DataMember]
            public int Value
            {
                get { return _value; }
                private set { Set( ref _value, value ); }
            }


            public SerializableInt( int value )
            {
                Value = value;
            }
        }

        [Fact]
        public void SubclassesCanBeDataContractSerialized()
        {
            var obj = new SerializableInt( 42 );

            var serializer = new DataContractJsonSerializer( obj.GetType() );
            var stream = new MemoryStream();

            serializer.WriteObject( stream, obj );

            stream.Seek( 0, SeekOrigin.Begin );
            var roundtrippedObj = (SerializableInt) serializer.ReadObject( stream );

            Assert.Equal( obj.Value, roundtrippedObj.Value );
        }
    }
}