using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Xml.Linq;
using apack.HelperClasses;
using apack.PerformanceRun;


namespace apack
{
    public class PerformanceRunViewModel : ObservableObject, IPageViewModel
    {
        #region Members

        string _indexName;
        string _nodeAddress;
        bool _nodeAccess;
        int _collectionTask;
        CancellationTokenSource _cts;

        #endregion

        #region Construction

        public PerformanceRunViewModel()
        {
            RunForHoursList = new List<int> { 4, 12, 24 };
            //DataCollection = new PerformanceRunModel();
            PerformanceRunModel.Instance.PropertyChanged += _run_PropertyChanged;
            SetDefaultSettings();
            CheckIndexExists();
            //NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress).Result;
        }
        #endregion

        #region Properties

        public string Name => "Performance Collector";

        //public PerformanceRunModel DataCollection { get; set; }

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

        public string Status => PerformanceRunModel.Instance.Running ? "Test running" : "Test not running";

        public string IndexName 
        {
            get
            {
                return _indexName = _indexName ?? "performance";
            }
            set
            {
                _indexName = value;
                CheckIndexExists();
                //NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress).Result;
                RaisePropertyChanged(string.Empty);
            } 
        }

        public string NodeAddress {
            get
            {
                return _nodeAddress = _nodeAddress ?? "http://localhost:9200";
            }
            set
            {
                if (_nodeAddress == value) return;

                _nodeAddress = value;
                CheckIndexExists();
                //NodeAccess = DataCollection.IndexExists(IndexName, NodeAddress).Result;
                RaisePropertyChanged(string.Empty);
            }
            
            }

        bool NodeAccess
        {
            get
            {
                return _nodeAccess;
            }
            set
            {
                _nodeAccess = value;
                RaisePropertyChanged(string.Empty);
            }
        }

        public string NodeAccessMessage => NodeAccess ? "Node accessible." : "Node not accessible";

        #endregion

        #region Methods
        void _run_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(string.Empty);
        }

        void SetDefaultSettings()
        {
            var settingsXml = XElement.Load(@"Resources\UserSettings.xml");

            var xElementDefaultNodeAddress = settingsXml.Element("DefaultNodeAddress");
            if (xElementDefaultNodeAddress != null)
                NodeAddress = xElementDefaultNodeAddress.Value;

            var xElementDefaultIndex = settingsXml.Element("DefaultIndex");
            if (xElementDefaultIndex != null)
                IndexName = xElementDefaultIndex.Value;
        }

        async void CheckIndexExists()
        {
            NodeAccess = await PerformanceRunModel.Instance.IndexExists(IndexName, NodeAddress);
        }

        #endregion

        #region Commands

        // Command for running the collector
        async void CollectDataExecute()
        {
            _cts = new CancellationTokenSource();
            
            PerformanceRunModel.Instance.IndexName = IndexName;
            PerformanceRunModel.Instance.NodeAddress = NodeAddress;
            PerformanceRunModel.Instance.RunningTime = RunningTime;

            _collectionTask = await PerformanceRunModel.Instance.Run(_cts.Token);
        }

        bool CanCollectDataExecute()
        {
            return !PerformanceRunModel.Instance.Running && NodeAccess;
        }

        public ICommand CollectData
        {
            get
            {
                return new RelayCommand(param => CollectDataExecute(), param => CanCollectDataExecute());
            }
        }

        // Command for testing hte connection to the node and index
        async void TestConnectionExecute()
        {
           await PerformanceRunModel.Instance.IndexExists(IndexName, NodeAddress);
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
