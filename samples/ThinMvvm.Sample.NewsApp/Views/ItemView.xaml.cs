using ThinMvvm.Sample.NewsApp.ViewModels;

namespace ThinMvvm.Sample.NewsApp.Views
{
    public sealed partial class ItemView
    {
        public ItemView()
        {
            InitializeComponent();

            // WebView's content can't be bound
            Loaded += ( _, __ ) =>
            {
                var vm = (ItemViewModel) DataContext;
                ContentView.NavigateToString( vm.Item.Description );
            };
        }
    }
}