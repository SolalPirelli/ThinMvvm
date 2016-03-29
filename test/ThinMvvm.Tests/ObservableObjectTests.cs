using System;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="ObservableObject" />.
    /// </summary>
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
    }
}