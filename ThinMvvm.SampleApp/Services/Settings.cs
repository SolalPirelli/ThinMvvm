// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System.Collections.ObjectModel;

namespace ThinMvvm.SampleApp.Services
{
    public sealed class Settings : SettingsBase<Settings>, ISettings
    {
        public ObservableCollection<string> ReadArticles
        {
            get { return Get<ObservableCollection<string>>(); }
            set { Set( value ); }
        }


        public Settings( ISettingsStorage storage ) : base( storage ) { }


        protected override SettingsDefaultValues GetDefaultValues()
        {
            return new SettingsDefaultValues
            {
                { x => x.ReadArticles, () => new ObservableCollection<string>() }
            };
        }
    }
}