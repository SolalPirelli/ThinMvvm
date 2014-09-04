// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Windows;
using Microsoft.Phone.Controls;
using ThinMvvm.SampleApp.ViewModels;

namespace ThinMvvm.SampleApp.Views
{
    public partial class NewsItemView : PhoneApplicationPage
    {
        public NewsItemView()
        {
            InitializeComponent();
            Loaded += This_Loaded;
        }

        private void This_Loaded( object sender, RoutedEventArgs e )
        {
            var vm = (NewsItemViewModel) DataContext;
            Browser.NavigateToString( vm.Item.Description );
        }
    }
}