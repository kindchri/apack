using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Nest;

namespace apack.HelperClasses
{
    class ElasticServer
    {
        public static readonly ElasticServer _instance = new ElasticServer();
        public ElasticClient ElasticServerClient { get; private set; }

        public bool IsClientSet => ElasticServerClient != null;

        public string NodeAddress { get; private set; }
        public string IndexName { get; private set; }

        /// <summary>
        /// Checks that the index at the given address exists and sets the ElasticClient if it does.
        /// </summary>
        /// <param name="indexname">Name of the index</param>
        /// <param name="address">Address to the node</param>
        /// <returns>True if successful and false if not.</returns>
        public async Task<bool> SetElasticServerClientAsync(string address, string indexname = "kibana")
        {
            var exists = await IndexExistsAsync(indexname, address);

            if (!exists)
                return false;

            var node = new Uri(address);
            var config = new ConnectionSettings(node);
            config.DefaultIndex(indexname);
            
            ElasticServerClient = new ElasticClient(config);
            NodeAddress = address;
            IndexName = indexname;

            return true;
        }


        public void SendToElasticSearch(PerformanceSample sample)
        {
            ElasticServerClient.Index(sample);
        }

        public async Task<bool> IndexExistsAsync(string indexname, string address)
        {
            try
            {
                var node = new Uri(address);
                var config = new ConnectionSettings(node);
                var client = new ElasticClient(config);

                Func<bool> checkFunc = () => client.IndexExists(indexname, i => i.Index(indexname)).Exists;
                var result = await Task.Run(checkFunc);
                return result;
            }
            catch (UriFormatException)
            {
                //MessageBox.Show("Invalid input format.");
                return false;
            }
        }

        public List<string> GetAllIndices()
        {
            var indexNameList = new List<string>();
            if (!IsClientSet)
            {
                return indexNameList;
            }

            var indices = ElasticServerClient.CatIndices();

            indexNameList.AddRange(indices.Records.Select(index => index.ToString()));

            return indexNameList;
        }
    }
}
