// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle( "ThinMvvm" )]
[assembly: AssemblyDescription( "MVVM framework for thin clients." )]
[assembly: AssemblyCompany( "Solal Pirelli" )]
[assembly: AssemblyCopyright( "Copyright © Solal Pirelli 2014" )]
[assembly: AssemblyCulture( "" )]

[assembly: CLSCompliant( true )]

[assembly: AssemblyVersion( "0.9.28" )]

[assembly: InternalsVisibleTo( "ThinMvvm.Logging" )]
[assembly: InternalsVisibleTo( "ThinMvvm.WindowsPhone" )]
[assembly: InternalsVisibleTo( "ThinMvvm.WindowsRuntime" )]

[assembly: InternalsVisibleTo( "ThinMvvm.Tests" )]