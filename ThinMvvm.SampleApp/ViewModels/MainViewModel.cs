// Copyright (c) Solal Pirelli 2014
// See License.txt file for more details

using System;
using ThinMvvm.SampleApp.Services;

namespace ThinMvvm.SampleApp.ViewModels
{
    public sealed class MainViewModel : ViewModel<int>
    {
        private readonly ISettings _settings;
        private readonly INavigationService _navigationService;


        public int Argument { get; private set; }

        private string _savedText;
        public string SavedText
        {
            get { return _savedText; }
            private set { SetProperty( ref _savedText, value ); }
        }


        public Command<string> SaveCommand
        {
            get { return this.GetCommand<string>( Save, s => !string.IsNullOrWhiteSpace( s ) ); }
        }

        public Command ShowAboutViewCommand
        {
            get { return this.GetCommand( _navigationService.NavigateTo<AboutViewModel> ); }
        }


        public MainViewModel( ISettings settings, INavigationService navigationService,
                              int arg )
        {
            _settings = settings;
            _navigationService = navigationService;

            Argument = arg;
        }


        public override void OnNavigatedTo()
        {
            SavedText = _settings.SavedText;
        }


        private void Save( string arg )
        {
            SavedText = arg;
            _settings.SavedText = SavedText + " - " + DateTime.Now.ToString();
        }
    }
}