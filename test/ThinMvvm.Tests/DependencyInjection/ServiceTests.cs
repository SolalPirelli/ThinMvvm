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
            private interface IDependency { }

            private sealed class DependentService
            {
                public DependentService( IDependency dependency ) { }
            }

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
                collection.AddSingleton<IDependency>( () => null );
                var creator = collection.BuildCreator();

                Assert.Throws<InvalidOperationException>( () => creator.Create( typeof( DependentService ), null ) );
            }

            [Fact]
            public void CannotCreateTransientServiceIfFactoryReturnsNull()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency>( () => null );
                var creator = collection.BuildCreator();

                Assert.Throws<InvalidOperationException>( () => creator.Create( typeof( DependentService ), null ) );
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
            private interface IDependency { }

            private sealed class Dependency : IDependency { }

            private sealed class DependentService
            {
                public IDependency Dependency { get; }

                public DependentService( IDependency dependency )
                {
                    Dependency = dependency;
                }
            }

            private sealed class ConcreteDependentService
            {
                public Dependency Dependency { get; }

                public ConcreteDependentService( Dependency dependency )
                {
                    Dependency = dependency;
                }
            }

            [Fact]
            public void CreateReturnsServiceInstance()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency();
                collection.AddInstance<IDependency>( instance );

                var created = (DependentService) collection.BuildCreator().Create( typeof( DependentService ), null );

                Assert.Same( instance, created.Dependency );
            }

            [Fact]
            public void CreateReturnsSingletonImplementation()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<IDependency, Dependency>();

                var created = (DependentService) collection.BuildCreator().Create( typeof( DependentService ), null );

                Assert.IsType( typeof( Dependency ), created.Dependency );
            }

            [Fact]
            public void CreateReturnsSameSingletonImplementationEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<IDependency, Dependency>();
                var creator = collection.BuildCreator();

                var created = (DependentService) creator.Create( typeof( DependentService ), null );
                var created2 = (DependentService) creator.Create( typeof( DependentService ), null );

                Assert.Same( created.Dependency, created2.Dependency );
            }

            [Fact]
            public void CreateReturnsSingletonService()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<Dependency>();

                var created = (ConcreteDependentService) collection.BuildCreator().Create( typeof( ConcreteDependentService ), null );

                Assert.IsType( typeof( Dependency ), created.Dependency );
            }

            [Fact]
            public void CreateReturnsSameSingletonServiceEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddSingleton<Dependency>();
                var creator = collection.BuildCreator();

                var created = (ConcreteDependentService) creator.Create( typeof( ConcreteDependentService ), null );
                var created2 = (ConcreteDependentService) creator.Create( typeof( ConcreteDependentService ), null );

                Assert.Same( created.Dependency, created2.Dependency );
            }

            [Fact]
            public void CreateReturnsSingletonFactoryResult()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency();
                collection.AddSingleton<IDependency>( () => instance );

                var created = (DependentService) collection.BuildCreator().Create( typeof( DependentService ), null );

                Assert.Same( instance, created.Dependency );
            }

            [Fact]
            public void CreateCallsSingletonFactoryOnlyOnce()
            {
                var collection = new ServiceCollection();
                var count = 0;
                collection.AddSingleton<IDependency>( () => { count++; return new Dependency(); } );
                var creator = collection.BuildCreator();

                creator.Create( typeof( DependentService ), null );
                creator.Create( typeof( DependentService ), null );

                Assert.Equal( 1, count );
            }

            [Fact]
            public void CreateReturnsTransientImplementation()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency, Dependency>();

                var created = (DependentService) collection.BuildCreator().Create( typeof( DependentService ), null );

                Assert.IsType( typeof( Dependency ), created.Dependency );
            }

            [Fact]
            public void CreateReturnsDifferentTransientImplementationEachTime()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency, Dependency>();
                var creator = collection.BuildCreator();

                var created = (DependentService) creator.Create( typeof( DependentService ), null );
                var created2 = (DependentService) creator.Create( typeof( DependentService ), null );

                Assert.NotSame( created.Dependency, created2.Dependency );
            }

            [Fact]
            public void CreateReturnsTransientFactoryResult()
            {
                var collection = new ServiceCollection();
                var instance = new Dependency();
                collection.AddTransient<IDependency>( () => instance );

                var created = (DependentService) collection.BuildCreator().Create( typeof( DependentService ), null );

                Assert.Same( instance, created.Dependency );
            }

            [Fact]
            public void CreateCallsTransientFactoryEachTime()
            {
                var collection = new ServiceCollection();
                var count = 0;
                collection.AddTransient<IDependency>( () => { count++; return new Dependency(); } );
                var creator = collection.BuildCreator();

                creator.Create( typeof( DependentService ), null );
                creator.Create( typeof( DependentService ), null );

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
            

            private interface IDependency2 { }

            private sealed class Dependency2 : IDependency2 { }

            private sealed class DependentService12
            {
                public readonly IDependency Dependency1;
                public readonly IDependency2 Dependency2;

                public DependentService12( IDependency dependency1, IDependency2 dependency2 )
                {
                    Dependency1 = dependency1;
                    Dependency2 = dependency2;
                }
            }

            [Fact]
            public void CreateCanInstantiateUnknownClassesWithMultipleDependencies()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency, Dependency>();
                collection.AddTransient<IDependency2, Dependency2>();

                var created = (DependentService12) collection.BuildCreator().Create( typeof( DependentService12 ), null );

                Assert.IsType( typeof( Dependency ), created.Dependency1 );
                Assert.IsType( typeof( Dependency2 ), created.Dependency2 );
            }


            private sealed class RecursivelyDependentService
            {
                public readonly DependentService12 Service;
                public readonly IDependency Dependency;

                public RecursivelyDependentService( DependentService12 service, IDependency dependency )
                {
                    Service = service;
                    Dependency = dependency;
                }
            }

            [Fact]
            public void CreateResolvesDependenciesRecursively()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency, Dependency>();
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
                public readonly IDependency Dependency;
                public readonly int Value;

                public IntDependentService( IDependency dependency, int value )
                {
                    Dependency = dependency;
                    Value = value;
                }
            }

            [Fact]
            public void CreateInstantiatesDependentServiceWithArgument()
            {
                var collection = new ServiceCollection();
                collection.AddTransient<IDependency, Dependency>();

                var service = (IntDependentService) collection.BuildCreator().Create( typeof( IntDependentService ), 100 );

                Assert.IsType( typeof( Dependency ), service.Dependency );
                Assert.Equal( 100, service.Value );
            }


            [Fact]
            public void ServiceCollectionDoesNotPropagateChangesToPreviousCreators()
            {
                var collection = new ServiceCollection();
                var creator = collection.BuildCreator();
                collection.AddTransient<IDependency, Dependency>();

                Assert.Throws<ArgumentException>( () => creator.Create( typeof( IDependency ), null ) );
            }
        }
    }
}