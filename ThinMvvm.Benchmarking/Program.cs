// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ThinMvvm.Benchmarking
{
    /* v0.9.1 (i7-3612QM)
     * Container: No dependencies/parameters        00:00:00.0000002
     * Container: One dependency                    00:00:00.0000003
     * Container: One parameter                     00:00:00.0000003
     * Container: One dependency & one parameter    00:00:00.0000003
     * Container: Many dependencies                 00:00:00.0000005
     * Container: Many dependencies & one parameter 00:00:00.0000007
     */

    public sealed class Program
    {
        private const int WarmupIterations = 10;
        private const int Iterations = 1000000;

        private static readonly Dictionary<string, Action> Actions = new Dictionary<string, Action>
        {
            { "Container: No dependencies/parameters", () => Container.Get( typeof( NoDependenciesOrParameters ), null ) },
            { "Container: One dependency", () => Container.Get( typeof( OneDependency ), null ) },
            { "Container: One parameter", () => Container.Get( typeof( OneParameter ), 0 ) },
            { "Container: One dependency & one parameter", () => Container.Get( typeof( OneDependencyAndOneParameter ), 0 ) },
            { "Container: Many dependencies", () => Container.Get( typeof( ManyDependencies ), null ) },
            { "Container: Many dependencies & one parameter", () => Container.Get( typeof( ManyDependenciesAndOneParameter ), 0 ) }
        };

        static Program()
        {
            Container.Bind<IDependency, Dependency>();
            Container.Bind<IDependency2, Dependency2>();
            Container.Bind<IDependency3, Dependency3>();
            Container.Bind<IDependency4, Dependency4>();
        }

        public static void Main( string[] args )
        {
            string format = string.Format( "{{0, -{0}}} {{1}}", Actions.Keys.Max( s => s.Length ) );
            foreach ( var pair in Actions )
            {
                var time = MeasureExecutionTime( pair.Value );
                Console.WriteLine( format, pair.Key, time.ToString() );
            }

            Console.Read();
        }

        private static TimeSpan MeasureExecutionTime( Action action )
        {
            for ( int n = 0; n < WarmupIterations; n++ )
            {
                action();
            }

            var watch = new Stopwatch();
            for ( int n = 0; n < Iterations; n++ )
            {
                watch.Start();
                action();
                watch.Stop();
            }

            return TimeSpan.FromTicks( watch.ElapsedTicks / Iterations );
        }
    }

    #region Dependencies and dependent classes for container benchmarks
    internal interface IDependency { }
    internal sealed class Dependency : IDependency { }
    internal interface IDependency2 { }
    internal sealed class Dependency2 : IDependency2 { }
    internal interface IDependency3 { }
    internal sealed class Dependency3 : IDependency3 { }
    internal interface IDependency4 { }
    internal sealed class Dependency4 : IDependency4 { }

    internal sealed class NoDependenciesOrParameters { }

    internal sealed class OneParameter
    {
        public OneParameter( int n ) { }
    }

    internal sealed class OneDependency
    {
        public OneDependency( IDependency dependency ) { }
    }

    internal sealed class OneDependencyAndOneParameter
    {
        public OneDependencyAndOneParameter( IDependency dependency, int n ) { }
    }

    internal sealed class ManyDependencies
    {
        public ManyDependencies( IDependency dep1, IDependency2 dep2, IDependency3 dep3, IDependency4 dep4 ) { }
    }

    internal sealed class ManyDependenciesAndOneParameter
    {
        public ManyDependenciesAndOneParameter( IDependency dep1, IDependency2 dep2, IDependency3 dep3, IDependency4 dep4, int n ) { }
    }
    #endregion
}