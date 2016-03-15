using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using apack_v0_9.HelperClasses;
using apack_v0_9.PerformanceRun;


namespace apack_v0_9
{
    public class PerformanceRunViewModel : ObservableObject, IPageViewModel
    {
        #region Members
        PerformanceRunModel _dataCollection;
        string _indexName;
        string _nodeAddress;

        int _collectionTask;
        CancellationTokenSource _cts;

        #endregion

        #region Construction
        public PerformanceRunViewModel()
        {
            RunForHoursList = new List<int> { 4, 12, 24 };
            DataCollection = new PerformanceRunModel(NodeAddress, IndexName, RunForHoursList[0]);
            DataCollection.PropertyChanged += _run_PropertyChanged;
            NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress);
        }
        #endregion

        #region Properties

        public string Name
        {
            get { return "Performance Collector"; }
        }
        
        public PerformanceRunModel DataCollection
        {
            get { return _dataCollection; }
            set { _dataCollection = value; }
        }

        public int RunningTime
        {
            get { return DataCollection.RunningTime; }
            set 
            {
                if (DataCollection.RunningTime != value)
                {
                    DataCollection.RunningTime = value;
                    RaisePropertyChanged("RunningTime");
                }
            }
        }

        public string LastPerfSample
        {
            get { return DataCollection.LastPerfSample; }
            set
            {
                if (DataCollection.LastPerfSample != value)
                {
                    DataCollection.LastPerfSample = value;
                    RaisePropertyChanged("LastPerfSample");
                }
            }
        }

        public List<int> RunForHoursList { get; }

        public string Status => DataCollection.Running ? "Test running" : "Test not running";

        public string IndexName 
        {
            get
            {
                return _indexName = _indexName ?? "performance";
            }
            set
            {
                if (_indexName != value)
                {
                    _indexName = value;
                    NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress);
                    RaisePropertyChanged(string.Empty);
                }
                
            } 
        }

        public string NodeAddress {
            get
            {
                return _nodeAddress = _nodeAddress ?? "http://localhost:9200";
            }
            set
            {
                if (_nodeAddress != value)
                {
                    _nodeAddress = value;
                    NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress);
                    RaisePropertyChanged(string.Empty);
                }
            }
            
            }

        bool NodeAccess { get; set; }

        public string NodeAccessMessage
        {
            get
            {
                if (NodeAccess)
                {
                    return "Node accessible.";
                }
                else
                {
                    return "Node not accessible";
                }
            }
        }
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
            _cts = new CancellationTokenSource();
            _collectionTask = await DataCollection.Run(_cts.Token);
        }

        bool CanCollectDataExecute()
        {
            if (DataCollection.Running)
            {
                return false;
            }
            else if (!NodeAccess)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public ICommand CollectData
        {
            get
            {
                return new RelayCommand(param => CollectDataExecute(), param => CanCollectDataExecute());
            }
        }

        // Command for testing hte connection to the node and index
        void TestConnectionExecute()
        {
            DataCollection.IndexExists(IndexName, NodeAddress);
        }

        bool CanTestConnectionExecute()
        {
            return true;
        }

        public ICommand TestConnection
        {
            get
            {
                return new RelayCommand(param => TestConnectionExecute(), param => CanTestConnectionExecute());
            }
        }

        // Command for cancelling the current collection 
        void CancelExecute()
        {
            if (_cts != null)
            {
                MessageBox.Show("Trying to cancel");
                _cts.Cancel();
            }
        }

        bool CanCancelExecute()
        {
            if (DataCollection.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
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
