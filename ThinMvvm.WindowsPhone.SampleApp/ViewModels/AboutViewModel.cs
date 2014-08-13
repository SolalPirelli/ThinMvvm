// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using System.Reflection;

namespace ThinMvvm.WindowsPhone.SampleApp.ViewModels
{
    public sealed class AboutViewModel : ViewModel<NoParameter>
    {
        public Version AppVersion { get; private set; }


        public AboutViewModel()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}