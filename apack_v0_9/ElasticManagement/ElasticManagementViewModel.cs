using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using apack.HelperClasses;
using System.Windows.Input;
using System.Xml.Linq;
using Nest;

//using ServiceStack.Commands;

namespace apack
{
    class ElasticManagementViewModel : ObservableObject, IPageViewModel
    {
        #region Construction

        public ElasticManagementViewModel()
        {
            SetDefaultSettings();
        }

        #endregion

        #region Properties
        public string Name => "ElasticManagement";

        public string NodeAddress { get; set; }
        
        public string IndexName { get; set; }
       
        public string CreateNewIndexName { get; set; }

        public List<string> IndexList { get; set; } 

        public string ChosenIndex { get; set; }

        #endregion

        #region Methods
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
        #endregion


        #region Commands

        // Create index methods and property
        async void CreateIndexExecute()
        {
            var result = await ElasticServer.Instance.CreateIndexAsync(IndexName);
            UpdateIndexListBoxExecute();
            MessageBox.Show(result ? "Index created!" : "Something went wrong");
        }

        bool CanCreateIndexExecute()
        {
            return ElasticServer.Instance.IsClientSet;
        }

        public ICommand CreateIndex
        {
            get
            {
                return new RelayCommand(param => CreateIndexExecute(), param => CanCreateIndexExecute());
            }
        }

        // Set client command methods and property
        async void SetClientExecute()
        {
            bool result = await ElasticServer.Instance.SetElasticServerClientAsync(NodeAddress);
            if (result) UpdateIndexListBoxExecute();
            if(!result) { MessageBox.Show("Node was not set, check the address and try pinging your node.");}

        }

        bool CanSetClient()
        {
            return true;
        }

        public ICommand SetClient
        {
            get
            {
                return new RelayCommand(param => SetClientExecute(), param => CanSetClient());
            }
        }

        void UpdateIndexListBoxExecute()
        {
            IndexList = ElasticServer.Instance.GetAllIndices();
            RaisePropertyChanged("");
        }

        bool CanUpdateIndexListBoxExecute()
        {
            return ElasticServer.Instance.IsClientSet;
        }

        public ICommand UpdateIndexListBox
        {
            get
            {
                return new RelayCommand(param => UpdateIndexListBoxExecute(), param => CanUpdateIndexListBoxExecute());
            }
        }

        async void SetIndexExecute()
        {
            var result = false;

            if (NodeAddress != null && ChosenIndex != null)
            {
                result = await ElasticServer.Instance.SetElasticServerClientAsync(NodeAddress, ChosenIndex);
            }

            MessageBox.Show(result
                ? $"Node: {ElasticServer.Instance.NodeAddress}, Index: {ElasticServer.Instance.IndexName}"
                : "Something went wrong, node and index not set");
        }

    
        

        bool CanSetIndexExecute()
        {
            return ElasticServer.Instance.IsClientSet;
        }

        public ICommand SetIndex
        {
            get
            {
                return new RelayCommand(param => SetIndexExecute(), param => CanSetIndexExecute());
            }
        }
        #endregion
    }
}
