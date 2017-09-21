using System.ComponentModel;
using ThinMvvm.Data.Infrastructure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ThinMvvm.Windows.Controls.Infrastructure
{
    /// <summary>
    /// Base control for displaying properties of a data source.
    /// </summary>
    public abstract class DataControlBase : Control
    {
        /// <summary>
        /// Gets or sets the data source whose properties should be displayed.
        /// </summary>
        public IDataSource DataSource
        {
            get { return (IDataSource) GetValue( DataSourceProperty ); }
            set { SetValue( DataSourceProperty, value ); }
        }

        /// <summary>
        /// Describes the <see cref="DataSource" /> property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register( nameof( DataSource ), typeof( IDataSource ), typeof( DataControlBase ), new PropertyMetadata( null, OnDataSourceChanged ) );

        /// <summary>
        /// Called when the data source changes.
        /// </summary>
        private static void OnDataSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var control = (DataControlBase) d;

            var oldSource = (IDataSource) e.OldValue;
            if( oldSource != null )
            {
                oldSource.PropertyChanged -= control.DataSourcePropertyChanged;
            }

            var source = (IDataSource) e.NewValue;
            if( source != null )
            {
                source.PropertyChanged += control.DataSourcePropertyChanged;
            }

            control.Update();
        }


        protected override void OnApplyTemplate()
        {
            Update();
        }


        /// <summary>
        /// Updates the control.
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// Called whenever <see cref="DataSource" /> has a property change.
        /// </summary>
        private void DataSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == nameof( DataSource.Status ) )
            {
                Update();
            }
        }
    }
}