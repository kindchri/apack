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
        void CreateIndexExecute()
        {
            
        }

        bool CanCreateIndex()
        {
            return true;
        }

        public ICommand CreateIndex
        {
            get
            {
                return new RelayCommand(param => CreateIndexExecute(), param => CanCreateIndex());
            }
        }

        // Set client command methods and property
        async void SetClientExecute()
        {
            bool result = await ElasticServer._instance.SetElasticServerClientAsync(NodeAddress);

            MessageBox.Show(result ? "Success" : "Fail");

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

        #endregion
    }
}
