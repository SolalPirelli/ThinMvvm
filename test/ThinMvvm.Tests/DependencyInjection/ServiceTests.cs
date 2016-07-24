using System;
using ThinMvvm.DependencyInjection;
using Xunit;

namespace ThinMvvm.Tests
{
    public sealed class ServiceTests
    {
        public sealed class AddErrors
        {
            private interface IDependency { }

            private abstract class AbstractDependency : IDependency { }

            private sealed class Dependency : IDependency { }


            [Fact]
            public void CannotAddNullInstance()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentNullException>( () => collection.AddInstance<IDependency>( null ) );
            }

            [Fact]
            public void CannotAddSingletonWithNullFactory()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentNullException>( () => collection.AddSingleton<IDependency>( null ) );
            }

            [Fact]
            public void CannotAddSingletonWithInterfaceService()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddSingleton<IDependency>() );
            }

            [Fact]
            public void CannotAddSingletonWithInterfaceImplementation()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddSingleton<IDependency, IDependency>() );
            }

            [Fact]
            public void CannotAddTransientWithNullFactory()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentNullException>( () => collection.AddTransient<IDependency>( null ) );
            }

            [Fact]
            public void CannotAddTransientWithInterfaceImplementation()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddTransient<IDependency, IDependency>() );
            }

            [Fact]
            public void CannotAddSingleton1WithAbstractClassService()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddSingleton<AbstractDependency>() );
            }

            [Fact]
            public void CannotAddSingletonWithAbstractClassImplementation()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddSingleton<IDependency, AbstractDependency>() );
            }

            [Fact]
            public void CannotAddTransientWithAbstractClassImplementation()
            {
                var collection = new ServiceCollection();

                Assert.Throws<ArgumentException>( () => collection.AddTransient<IDependency, AbstractDependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsInstance()
            {
                var collection = new ServiceCollection();

                collection.AddInstance<IDependency>( new Dependency() );

                Assert.Throws<InvalidOperationException>( () => collection.AddInstance<IDependency>( new Dependency() ) );
            }

            [Fact]
            public void CannotAddServiceTwiceAsInstanceThenSingleton()
            {
                var collection = new ServiceCollection();

                collection.AddInstance<IDependency>( new Dependency() );

                Assert.Throws<InvalidOperationException>( () => collection.AddSingleton<IDependency, Dependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsInstanceThenTransient()
            {
                var collection = new ServiceCollection();

                collection.AddInstance<IDependency>( new Dependency() );

                Assert.Throws<InvalidOperationException>( () => collection.AddTransient<IDependency, Dependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsSingleton()
            {
                var collection = new ServiceCollection();

                collection.AddSingleton<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddSingleton<IDependency, Dependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsSingletonThenInstance()
            {
                var collection = new ServiceCollection();

                collection.AddSingleton<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddInstance<IDependency>( new Dependency() ) );
            }

            [Fact]
            public void CannotAddServiceTwiceAsSingletonThenTransient()
            {
                var collection = new ServiceCollection();

                collection.AddSingleton<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddTransient<IDependency, Dependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsTransient()
            {
                var collection = new ServiceCollection();

                collection.AddTransient<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddTransient<IDependency, Dependency>() );
            }

            [Fact]
            public void CannotAddServiceTwiceAsTransientThenInstance()
            {
                var collection = new ServiceCollection();

                collection.AddTransient<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddInstance<IDependency>( new Dependency() ) );
            }

            [Fact]
            public void CannotAddServiceTwiceAsTransientThenTransient()
            {
                var collection = new ServiceCollection();

                collection.AddTransient<IDependency, Dependency>();

                Assert.Throws<InvalidOperationException>( () => collection.AddSingleton<IDependency, Dependency>() );
            }
        }

        public sealed class CreateErrors
        {
            [Fact]
            public void CannotCreateNullType()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentNullException>( () => creator.Create( null, 0 ) );
            }

            [Fact]
            public void CannotCreateSingletonServiceIfFactoryReturnsNull()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<object>( () => null );
                var creator = collection.BuildCreator();

                Assert.Throws<InvalidOperationException>( () => creator.Create( typeof( object ), null ) );
            }

            [Fact]
            public void CannotCreateTransientServiceIfFactoryReturnsNull()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<object>( () => null );
                var creator = collection.BuildCreator();

                Assert.Throws<InvalidOperationException>( () => creator.Create( typeof( object ), null ) );
            }


            private interface IDependency { }

            private sealed class DependentService
            {
                public DependentService( IDependency dependency ) { }
            }

            [Fact]
            public void CannotCreateTypeWithUnknownDependency()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( DependentService ), null ) );
            }


            private sealed class NoPublicConstructorService
            {
                private NoPublicConstructorService() { }
            }

            [Fact]
            public void CannotCreateTypeWithNoPublicConstructor()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( NoPublicConstructorService ), null ) );
            }


            private sealed class MultipleConstructorsService
            {
                public readonly int Value;

                public MultipleConstructorsService()
                {
                    Value = 0;
                }

                public MultipleConstructorsService( int value )
                {
                    Value = value;
                }
            }

            [Fact]
            public void CannotCreateTypeWithMultiplePublicConstructors()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( MultipleConstructorsService ), null ) );
            }


            private sealed class IntService
            {
                public readonly int Value;

                public IntService( int value )
                {
                    Value = value;
                }
            }

            [Fact]
            public void CannotCreateWhenArgumentIsNotProvided()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( IntService ), null ) );
            }

            [Fact]
            public void CannotCreateWhenArgumentIsOfWrongType()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( IntService ), "hello" ) );
            }


            private sealed class TwoArgumentsService
            {
                public readonly int Value1;
                public readonly int Value2;

                public TwoArgumentsService( int value1, int value2 )
                {
                    Value1 = value1;
                    Value2 = value2;
                }
            }

            [Fact]
            public void CannotCreateWhenTooManyArgumentsAreExpected()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( TwoArgumentsService ), 42 ) );
            }


            private sealed class RecursivelyDependentService
            {
                public readonly IntService Service;
                public readonly int Value;

                public RecursivelyDependentService( IntService service, int value )
                {
                    Service = service;
                    Value = value;
                }
            }

            [Fact]
            public void CannotCreateRecursiveDependencyRequiringArgument()
            {
                var creator = new ServiceCollection().BuildCreator();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( RecursivelyDependentService ), 42 ) );
            }
        }

        public sealed class Create
        {
            private sealed class RecursivelyDependentService
            {
                public readonly DependentService12 Service;
                public readonly IDependency1 Dependency;

                public RecursivelyDependentService( DependentService12 service, IDependency1 dependency )
                {
                    Service = service;
                    Dependency = dependency;
                }
            }

            private sealed class SinglePublicConstructorService
            {
                public readonly int Value;

                public SinglePublicConstructorService()
                {
                    Value = 0;
                }

                private SinglePublicConstructorService( int value )
                {
                    Value = value;
                }
            }


            private interface IDependency1 { }

            private sealed class Dependency1 : IDependency1 { }

            [Fact]
            public void CreateReturnsServiceInstance()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency1();
                collection.AddInstance<IDependency1>( instance );

                var created = collection.BuildCreator().Create( typeof( IDependency1 ), null );

                Assert.Same( instance, created );
            }

            [Fact]
            public void CreateReturnsSingletonImplementation()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<IDependency1, Dependency1>();

                var created = collection.BuildCreator().Create( typeof( IDependency1 ), null );

                Assert.IsType( typeof( Dependency1 ), created );
            }

            [Fact]
            public void CreateReturnsSameSingletonImplementationEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<IDependency1, Dependency1>();
                var creator = collection.BuildCreator();

                var created = creator.Create( typeof( IDependency1 ), null );
                var created2 = creator.Create( typeof( IDependency1 ), null );

                Assert.Same( created, created2 );
            }

            [Fact]
            public void CreateReturnsSingletonService()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<Dependency1>();

                var created = collection.BuildCreator().Create( typeof( Dependency1 ), null );

                Assert.IsType( typeof( Dependency1 ), created );
            }

            [Fact]
            public void CreateReturnsSameSingletonServiceEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<Dependency1>();
                var creator = collection.BuildCreator();

                var created = creator.Create( typeof( Dependency1 ), null );
                var created2 = creator.Create( typeof( Dependency1 ), null );

                Assert.Same( created, created2 );
            }

            [Fact]
            public void CreateReturnsSingletonFactoryResult()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency1();
                collection.AddSingleton<IDependency1>( () => instance );

                var created = collection.BuildCreator().Create( typeof( IDependency1 ), null );

                Assert.Same( instance, created );
            }

            [Fact]
            public void CreateCallsSingletonFactoryOnlyOnce()
            {
                var collection = new ServiceCollection();
                var count = 0;
                collection.AddSingleton<IDependency1>( () => { count++; return new Dependency1(); } );
                var creator = collection.BuildCreator();

                creator.Create( typeof( IDependency1 ), null );
                creator.Create( typeof( IDependency1 ), null );

                Assert.Equal( 1, count );
            }

            [Fact]
            public void CreateReturnsTransientImplementation()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();

                var created = collection.BuildCreator().Create( typeof( IDependency1 ), null );

                Assert.IsType( typeof( Dependency1 ), created );
            }

            [Fact]
            public void CreateReturnsDifferentTransientImplementationEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();
                var creator = collection.BuildCreator();

                var created = creator.Create( typeof( IDependency1 ), null );
                var created2 = creator.Create( typeof( IDependency1 ), null );

                Assert.NotSame( created, created2 );
            }

            [Fact]
            public void CreateReturnsTransientFactoryResult()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency1();
                collection.AddTransient<IDependency1>( () => instance );

                var created = collection.BuildCreator().Create( typeof( IDependency1 ), null );

                Assert.Same( instance, created );
            }

            [Fact]
            public void CreateCallsTransientFactoryEachTime()
            {
                var collection = new ServiceCollection();
                var count = 0;
                collection.AddTransient<IDependency1>( () => { count++; return new Dependency1(); } );
                var creator = collection.BuildCreator();

                creator.Create( typeof( IDependency1 ), null );
                creator.Create( typeof( IDependency1 ), null );

                Assert.Equal( 2, count );
            }


            private sealed class IndependentService { }

            [Fact]
            public void CreateCanInstantiateUnknownIndependentClasses()
            {
                var creator = new ServiceCollection().BuildCreator();

                var created = creator.Create( typeof( IndependentService ), null );

                Assert.IsType( typeof( IndependentService ), created );
            }

            [Fact]
            public void CreateInstantiatesNewUnknownIndependentClassEachTime()
            {
                var creator = new ServiceCollection().BuildCreator();

                var created = creator.Create( typeof( IndependentService ), null );
                var created2 = creator.Create( typeof( IndependentService ), null );

                Assert.NotSame( created, created2 );
            }


            private sealed class DependentService1
            {
                public readonly IDependency1 Dependency;

                public DependentService1( IDependency1 dependency )
                {
                    Dependency = dependency;
                }
            }

            [Fact]
            public void CreateCanInstantiateUnknownClassesWithOneDependency()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();

                var created = (DependentService1) collection.BuildCreator().Create( typeof( DependentService1 ), null );

                Assert.IsType( typeof( Dependency1 ), created.Dependency );
            }

            [Fact]
            public void CreateCanInstantiateUnknownClassesWithOneInstanceDependency()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency1();
                collection.AddInstance<IDependency1>( instance );

                var created = (DependentService1) collection.BuildCreator().Create( typeof( DependentService1 ), null );

                Assert.Same( instance, created.Dependency );
            }


            private interface IDependency2 { }

            private sealed class Dependency2 : IDependency2 { }

            private sealed class DependentService12
            {
                public readonly IDependency1 Dependency1;
                public readonly IDependency2 Dependency2;

                public DependentService12( IDependency1 dependency1, IDependency2 dependency2 )
                {
                    Dependency1 = dependency1;
                    Dependency2 = dependency2;
                }
            }

            [Fact]
            public void CreateCanInstantiateUnknownClassesWithMultipleDependencies()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();
                collection.AddTransient<IDependency2, Dependency2>();

                var created = (DependentService12) collection.BuildCreator().Create( typeof( DependentService12 ), null );

                Assert.IsType( typeof( Dependency1 ), created.Dependency1 );
                Assert.IsType( typeof( Dependency2 ), created.Dependency2 );
            }

            [Fact]
            public void CreateResolvesDependenciesRecursively()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();
                collection.AddTransient<IDependency2, Dependency2>();

                var created = (RecursivelyDependentService) collection.BuildCreator().Create( typeof( RecursivelyDependentService ), null );

                Assert.NotNull( created.Dependency );
                Assert.NotNull( created.Service );
                Assert.NotNull( created.Service.Dependency1 );
            }


            private sealed class IntService
            {
                public readonly int Value;

                public IntService( int value )
                {
                    Value = value;
                }
            }

            [Fact]
            public void CreateInstantiatesIndependentServiceWithArgument()
            {
                var creator = new ServiceCollection().BuildCreator();

                var service = (IntService) creator.Create( typeof( IntService ), 42 );

                Assert.Equal( 42, service.Value );
            }


            private sealed class IntDependentService
            {
                public readonly IDependency1 Dependency;
                public readonly int Value;

                public IntDependentService( IDependency1 dependency, int value )
                {
                    Dependency = dependency;
                    Value = value;
                }
            }

            [Fact]
            public void CreateInstantiatesDependentServiceWithArgument()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency1, Dependency1>();

                var service = (IntDependentService) collection.BuildCreator().Create( typeof( IntDependentService ), 100 );

                Assert.IsType( typeof( Dependency1 ), service.Dependency );
                Assert.Equal( 100, service.Value );
            }


            [Fact]
            public void ServiceCollectionDoesNotPropagateChangesToPreviousCreators()
            {
                var collection = new ServiceCollection();
                var creator = collection.BuildCreator();
                collection.AddTransient<IDependency1, Dependency1>();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( IDependency1 ), null ) );
            }
        }
    }
}