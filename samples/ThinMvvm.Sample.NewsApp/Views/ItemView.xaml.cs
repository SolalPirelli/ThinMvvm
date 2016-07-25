using ThinMvvm.Sample.NewsApp.ViewModels;

namespace ThinMvvm.Sample.NewsApp.Views
{
    public sealed partial class ItemView
    {
        public ItemView()
        {
            InitializeComponent();

            DataContextChanged += ( _, __ ) =>
            {
                if( DataContext != null )
                {
                    // WebView's content can't be bound
                    var vm = (ItemViewModel) DataContext;
                    ContentView.NavigateToString( vm.Item.Description );
                }
            };
        }
    }
}