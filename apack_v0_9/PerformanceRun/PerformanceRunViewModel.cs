using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using apack.HelperClasses;
using apack.PerformanceRun;
using ServiceStack.Html;


namespace apack
{
    public class PerformanceRunViewModel : ObservableObject, IPageViewModel
    {
        #region Members
        int _collectionTask;
        CancellationTokenSource _cts;

        #endregion

        #region Construction

        public PerformanceRunViewModel()
        {
            RunForHoursList = new List<int> { 4, 12, 24, 9999 };
            PerformanceRunModel.Instance.PropertyChanged += _run_PropertyChanged;
            
        }
        #endregion

        #region Properties

        public string Name => "Performance Collector";

        public int RunningTime
        {
            get { return PerformanceRunModel.Instance.RunningTime; }
            set 
            {
                PerformanceRunModel.Instance.RunningTime = value;
                RaisePropertyChanged("RunningTime");
            }
        }

        public string LastPerfSample
        {
            get { return PerformanceRunModel.Instance.LastPerfSample; }
            set
            {
                PerformanceRunModel.Instance.LastPerfSample = value;
                RaisePropertyChanged("LastPerfSample"); 
            }
        }

        public List<int> RunForHoursList { get; }

        public string Status { get; set; }

        public string IndexName => ElasticServer.Instance.IndexName;

        public string NodeAddress => ElasticServer.Instance.NodeAddress;

       

        #endregion

        #region Methods
        void _run_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(string.Empty);
        }

        

        #endregion

        #region Commands

        // Command for running the collector
        async void CollectDataExecute()
        {
            try
            {
                _cts = new CancellationTokenSource();

                //PerformanceRunModel.Instance.IndexName = IndexName;
                //PerformanceRunModel.Instance.NodeAddress = NodeAddress;
                PerformanceRunModel.Instance.RunningTime = RunningTime;
                Status = "Collecting data";
                _collectionTask = await PerformanceRunModel.Instance.Run(_cts.Token);
                Status = "Finished collecting data";
            }
            catch (OperationCanceledException)
            {
                TestCancelled();
            }
        }

        void TestCancelled()
        {
            Status = "The test was cancelled";
            LastPerfSample = string.Empty;
            RaisePropertyChanged(string.Empty);
        }

        bool CanCollectDataExecute()
        {
            return !PerformanceRunModel.Instance.Running;
        }

        public ICommand CollectData
        {
            get
            {
                return new RelayCommand(param => CollectDataExecute(), param => CanCollectDataExecute());
            }
        }

        // Command for cancelling the current collection 
        void CancelExecute()
        {
            if (_cts == null) return;
            MessageBox.Show("Trying to cancel");
            _cts.Cancel();
            
        }

        bool CanCancelExecute()
        {
            return PerformanceRunModel.Instance.Running;
        }

        public ICommand Cancel
        {
            get
            {
                return new RelayCommand(param => CancelExecute(), param => CanCancelExecute());
            }
        }
        #endregion
    }
}
