namespace OnlyV.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;

    internal class MainViewModel : ViewModelBase
    {
        private readonly ScripturesViewModel _scripturesViewModel;
        private readonly PreviewViewModel _previewViewModel;

        private ViewModelBase _currentPage;

        public MainViewModel(
            ScripturesViewModel scripturesViewModel,
            PreviewViewModel previewViewModel)
        {
            _scripturesViewModel = scripturesViewModel;
            _previewViewModel = previewViewModel;

            InitCommands();
            _currentPage = scripturesViewModel;
        }

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public RelayCommand NextPageCommand { get; set; }

        public RelayCommand BackPageCommand { get; set; }

        private void InitCommands()
        {
            NextPageCommand = new RelayCommand(OnNext, CanDoNext);
            BackPageCommand = new RelayCommand(OnBack, CanDoBack);
        }

        private bool CanDoBack()
        {
            return CurrentPage != _scripturesViewModel;
        }

        private void OnBack()
        {
            if (CurrentPage == _previewViewModel)
            {
                CurrentPage = _scripturesViewModel;
            }
        }

        private bool CanDoNext()
        {
            // todo:

            if (CurrentPage == _scripturesViewModel)
            {
                return _scripturesViewModel.ValidScripture();
            }

            if (CurrentPage == _previewViewModel)
            {
                return false;
            }

            return false;
        }

        private void OnNext()
        {
            if (CurrentPage == _scripturesViewModel)
            {
                CurrentPage = _previewViewModel;
            }
        }
    }
}