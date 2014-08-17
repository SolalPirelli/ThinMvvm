﻿// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThinMvvm.Tests
{
    [TestClass]
    public sealed class ViewModelTests
    {
        private class TestViewModel : ViewModel<NoParameter> { }

        [TestMethod]
        public void OnNavigatedToWorks()
        {
            new TestViewModel().OnNavigatedTo();
        }

        [TestMethod]
        public void OnNavigatedFromWorks()
        {
            new TestViewModel().OnNavigatedFrom();
        }
    }
}