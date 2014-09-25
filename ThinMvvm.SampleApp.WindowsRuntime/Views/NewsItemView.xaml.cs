// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using ThinMvvm.SampleApp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.SampleApp.Views
{
    public sealed partial class NewsItemView : Page
    {
        public NewsItemView()
        {
            InitializeComponent();
            Loaded += This_Loaded;
        }

        private void This_Loaded( object sender, RoutedEventArgs e )
        {
            var vm = (NewsItemViewModel) DataContext;
            WebView.NavigateToString( vm.Item.Description );
        }
    }
}