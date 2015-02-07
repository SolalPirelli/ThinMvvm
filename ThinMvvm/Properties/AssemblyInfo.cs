// Copyright (c) 2014-15 Solal Pirelli
// See License.txt file for more details

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle( "ThinMvvm" )]
[assembly: AssemblyDescription( "MVVM framework for thin clients." )]
[assembly: AssemblyCompany( "Solal Pirelli" )]
[assembly: AssemblyCopyright( "Copyright © 2014-15 Solal Pirelli" )]
[assembly: AssemblyCulture( "" )]

[assembly: CLSCompliant( true )]

[assembly: AssemblyVersion( "0.10.4" )]

[assembly: InternalsVisibleTo( "ThinMvvm.Logging" )]
[assembly: InternalsVisibleTo( "ThinMvvm.WindowsPhone" )]
[assembly: InternalsVisibleTo( "ThinMvvm.WindowsRuntime" )]

[assembly: InternalsVisibleTo( "ThinMvvm.Tests" )]