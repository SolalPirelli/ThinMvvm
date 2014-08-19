// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Windows.Data;
using ThinMvvm.WindowsPhone.SampleApp.Resources;

namespace ThinMvvm.WindowsPhone.SampleApp
{
    public sealed class Text : Binding
    {
        private static AppResources _resources = new AppResources();

        public Text( string path )
            : base( path )
        {
            Source = _resources;
        }
    }
}