// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

namespace ThinMvvm.SampleApp.WindowsPhone.Services
{
    public sealed class Settings : SettingsBase<Settings>, ISettings
    {
        public string SavedText
        {
            get { return Get<string>(); }
            set { Set( value ); }
        }

        public Settings( ISettingsStorage storage ) : base( storage ) { }


        protected override SettingsDefaultValues GetDefaultValues()
        {
            return new SettingsDefaultValues
            {
                { x => x.SavedText, () => "Hello, World!" }
            };
        }
    }
}