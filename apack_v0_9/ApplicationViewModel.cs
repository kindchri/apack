using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using apack.HelperClasses;

namespace apack
{
    public class ApplicationViewModel : ObservableObject
    {
        #region Members
        private ICommand _changePageCommand;
        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;
        #endregion

        public ApplicationViewModel()
        {
            //PageViewModels.Add(new HomeViewModel());
            PageViewModels.Add(new ElasticManagementViewModel());
            PageViewModels.Add(new PerformanceRunViewModel());
            
            CurrentPageViewModel = PageViewModels[0];
        }

        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    if (_changePageCommand == null)
                    {
                        _changePageCommand = new RelayCommand(
                        param => ChangeViewModel((IPageViewModel)param),
                        param => CanChangeViewModel((IPageViewModel)param));
                            
                    }
                }
                return _changePageCommand;
            }
        }

        public List<IPageViewModel> PageViewModels
        {
            get 
            {
                if (_pageViewModels == null)
                {
                    _pageViewModels = new List<IPageViewModel>();
                }
                
                return _pageViewModels; 
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get { return _currentPageViewModel; }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    RaisePropertyChanged("CurrentPageViewModel");
                }
            }
        }

        #region Methods
        void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }

        bool CanChangeViewModel(IPageViewModel viewModel)
        {
            if (viewModel.Name == "Performance Collector")
            {
                return ElasticServer.Instance.IsClientSet;
            }
            return true;
        }

        #endregion


    }
}
