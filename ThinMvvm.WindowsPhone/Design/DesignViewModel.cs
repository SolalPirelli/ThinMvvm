// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Windows.Data;

namespace ThinMvvm.WindowsPhone.Design
{
    /// <summary>
    /// Utility class to easily mock data for a ViewModel at design-time.
    /// This class can be used as a markup extension in XAML, e.g. <c>{my:DesignViewModel}</c>.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
    /// <typeparam name="TParameter">The ViewModel's parameter type.</typeparam>
    public abstract class DesignViewModel<TViewModel, TParameter> : Binding
        where TViewModel : ViewModel<TParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesignViewModel{TViewModel, TParameter}" /> class.
        /// </summary>
        public DesignViewModel()
        {
            var vm = ViewModel; // so it doesn't need to be cached
            Source = vm;
            vm.OnNavigatedTo();
        }

        /// <summary>
        /// Gets a ViewModel instance mocked with design-time data.
        /// </summary>
        /// <remarks>
        /// This property is virtual; override it in debug builds only, so that the release builds do not contain extra code.
        /// </remarks>
        protected virtual TViewModel ViewModel
        {
            get { return default( TViewModel ); }
        }
    }
}