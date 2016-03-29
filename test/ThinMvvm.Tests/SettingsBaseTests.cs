using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Xunit;

namespace ThinMvvm.Tests
{
    /// <summary>
    /// Tests for <see cref="SettingsBase" />.
    /// The <see cref="GC.KeepAlive" /> method is used to force evaluation of arguments, without actually using them.
    /// </summary>
    public sealed class SettingsBaseTests
    {
        private sealed class InMemoryStore : IKeyValueStore
        {
            private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

            public int ReadCount { get; private set; }
            public int WriteCount { get; private set; }

            public Optional<T> Get<T>( string key )
            {
                ReadCount++;

                object boxedValue;
                if( _values.TryGetValue( key, out boxedValue ) )
                {
                    return new Optional<T>( (T) boxedValue );
                }

                return new Optional<T>();
            }

            public void Set<T>( string key, T value )
            {
                WriteCount++;

                _values[key] = value;
            }
        }

        private sealed class ObservableInt : ObservableObject
        {
            private int _value;

            public int Value
            {
                get { return _value; }
                set { Set( ref _value, value ); }
            }
        }

        // Type that only implements INotifyCollectionChanged and not INotifyPropertyChanged
        private sealed class ObservableBlackHole : INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void Eat( int n )
            {
                CollectionChanged?.Invoke( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
            }
        }

        private sealed class SpecialObservableCollection : ObservableCollection<int>
        {
            private int _value;

            public int Value
            {
                get { return _value; }
                set
                {
                    if( value != _value )
                    {
                        _value = value;
                        OnPropertyChanged( new PropertyChangedEventArgs( nameof( Value ) ) );
                    }
                }
            }
        }

        private sealed class Settings : SettingsBase
        {
            public bool ComputedLazyDefault { get; private set; }

            public int IntWithDefault
            {
                get { return Get( 100 ); }
                set { Set( value ); }
            }

            public int IntWithLazyDefault
            {
                get { return Get( () => { ComputedLazyDefault = true; return 200; } ); }
                set { Set( value ); }
            }

            public ObservableInt ObservableInt
            {
                get { return Get( new ObservableInt() ); }
                set { Set( value ); }
            }

            public ObservableBlackHole ObservableBlackHole
            {
                get { return Get( new ObservableBlackHole() ); }
                set { Set( value ); }
            }

            public ObservableCollection<int> ObservableCollection
            {
                get { return Get( new ObservableCollection<int>() ); }
                set { Set( value ); }
            }

            public SpecialObservableCollection SpecialObservableCollection
            {
                get { return Get( new SpecialObservableCollection() ); }
                set { Set( value ); }
            }

            public Settings() : this( new InMemoryStore() ) { }

            public Settings( IKeyValueStore store ) : base( store ) { }


            public int GetNullName()
            {
                return Get( 0, null );
            }

            public int GetLazyNullName()
            {
                return Get( () => 0, null );
            }

            public void SetNullName()
            {
                Set( 0, null );
            }
        }

        [Fact]
        public void CannotCreateSettingsWithNullStore()
        {
            Assert.Throws<ArgumentNullException>( () => new Settings( null ) );
        }

        [Fact]
        public void GetReturnsValueFromStore()
        {
            var store = new InMemoryStore();
            store.Set( typeof( Settings ).FullName + "." + nameof( Settings.IntWithDefault ), 42 );

            var settings = new Settings( store );

            Assert.Equal( 42, settings.IntWithDefault );
        }

        [Fact]
        public void SetPutsValueInStore()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );

            settings.IntWithDefault = 42;

            Assert.Equal( new Optional<int>( 42 ), store.Get<int>( typeof( Settings ).FullName + "." + nameof( Settings.IntWithDefault ) ) );
        }

        [Fact]
        public void GetReturnsSetValue()
        {
            var settings = new Settings();

            settings.IntWithDefault = 42;

            Assert.Equal( 42, settings.IntWithDefault );
        }

        [Fact]
        public void CannotPassNullNameToGet()
        {
            var settings = new Settings();

            Assert.Throws<ArgumentNullException>( () => settings.GetNullName() );
        }

        [Fact]
        public void CannotPassNullNameToLazyGet()
        {
            var settings = new Settings();

            Assert.Throws<ArgumentNullException>( () => settings.GetLazyNullName() );
        }

        [Fact]
        public void CannotPassNullNameToSet()
        {
            var settings = new Settings();

            Assert.Throws<ArgumentNullException>( () => settings.SetNullName() );
        }

        [Fact]
        public void DefaultValueIsReturned()
        {
            var settings = new Settings();

            Assert.Equal( 100, settings.IntWithDefault );
        }

        [Fact]
        public void DefaultValueIsOverwritenBySet()
        {
            var settings = new Settings();

            settings.IntWithDefault = 42;

            Assert.Equal( 42, settings.IntWithDefault );
        }

        [Fact]
        public void LazyDefaultValueIsReturned()
        {
            var settings = new Settings();

            Assert.Equal( 200, settings.IntWithLazyDefault );
        }

        [Fact]
        public void LazyDefaultValueIsOverwritenBySet()
        {
            var settings = new Settings();

            settings.IntWithDefault = 42;

            Assert.Equal( 200, settings.IntWithLazyDefault );
        }

        [Fact]
        public void LazyDefaultIsLazy()
        {
            var settings = new Settings();

            settings.IntWithLazyDefault = 42;
            GC.KeepAlive( settings.IntWithLazyDefault );

            Assert.False( settings.ComputedLazyDefault );
        }

        [Fact]
        public void DefaultValueIsNotImmediatelyStored()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );

            GC.KeepAlive( settings.IntWithDefault );

            Assert.Equal( 0, store.WriteCount );
        }

        [Fact]
        public void LazyDefaultValueIsImmediatelyStored()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );

            GC.KeepAlive( settings.IntWithLazyDefault );

            Assert.Equal( 1, store.WriteCount );
        }

        [Fact]
        public void ValuePropertyChangedTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            settings.ObservableInt = new ObservableInt();
            int writes = store.WriteCount;

            settings.ObservableInt.Value = 42;

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void ValueCollectionChangedTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            settings.ObservableBlackHole = new ObservableBlackHole();
            int writes = store.WriteCount;

            settings.ObservableBlackHole.Eat( 0 );

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void ValuePropertyChangedFollowingCollectionChangedDoesNotTriggerSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            settings.ObservableCollection = new ObservableCollection<int>();
            int writes = store.WriteCount;

            settings.ObservableCollection.Add( 0 );

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void ValuePropertyChangedOnPropertyUnrelatedToCollectionTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            settings.SpecialObservableCollection = new SpecialObservableCollection();
            int writes = store.WriteCount;

            settings.SpecialObservableCollection.Value = 1;

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void DefaultValuePropertyChangedTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            int writes = store.WriteCount;

            settings.ObservableInt.Value = 42;

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void DefaultValueCollectionChangedTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            int writes = store.WriteCount;

            settings.ObservableBlackHole.Eat( 0 );

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void DefaultValuePropertyChangedFollowingCollectionChangedDoesNotTriggerSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            int writes = store.WriteCount;

            settings.ObservableCollection.Add( 0 );

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void DefaultValuePropertyChangedOnPropertyUnrelatedToCollectionTriggersSet()
        {
            var store = new InMemoryStore();
            var settings = new Settings( store );
            int writes = store.WriteCount;

            settings.SpecialObservableCollection.Value = 1;

            Assert.Equal( writes + 1, store.WriteCount );
        }

        [Fact]
        public void ReadValuesAreCached()
        {
            var store = new InMemoryStore();
            store.Set( typeof( Settings ).FullName + "." + nameof( Settings.IntWithDefault ), 0 );
            var settings = new Settings( store );
            int reads = store.ReadCount;

            GC.KeepAlive( settings.IntWithDefault );
            GC.KeepAlive( settings.IntWithDefault );

            Assert.Equal( reads + 1, store.ReadCount );
        }

        [Fact]
        public void PropertyChangedIsTriggeredOnSet()
        {
            var settings = new Settings();
            bool fired = false;
            settings.PropertyChanged += ( _, e ) =>
            {
                if( e.PropertyName == nameof( Settings.IntWithDefault ) )
                {
                    fired = true;
                }
            };

            settings.IntWithDefault = 42;

            Assert.True( fired );
        }

        [Fact]
        public void PropertyChangedIsNotTriggeredIfNoChangeWasMade()
        {
            var settings = new Settings();
            int counter = 0;
            settings.PropertyChanged += ( _, e ) =>
            {
                if( e.PropertyName == nameof( Settings.IntWithDefault ) )
                {
                    counter++;
                }
            };

            settings.IntWithDefault = 42;
            settings.IntWithDefault = 42;

            Assert.Equal( 1, counter );
        }
    }
}