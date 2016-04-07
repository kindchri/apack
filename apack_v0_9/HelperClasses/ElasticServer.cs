using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nest;


namespace apack.HelperClasses
{
    class ElasticServer
    {
        readonly string _debugLogPath = Directory.GetCurrentDirectory() + "\\elasticserverlog.txt";

        public static readonly ElasticServer Instance = new ElasticServer();
        public ElasticClient ElasticServerClient { get; private set; }

        public bool IsClientSet => ElasticServerClient != null;

        public string NodeAddress { get; private set; }

        public string IndexName { get; private set; }

        /// <summary>
        /// Checks that the index at the given address exists async and sets the ElasticClient if it does.
        /// </summary>
        /// <param name="indexname">Name of the index</param>
        /// <param name="address">Address to the node</param>
        /// <returns>True if successful and false if not.</returns>
        public async Task<bool> SetElasticServerClientAsync(string address, string indexname = "default")
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

        /// <summary>
        /// Creates and index on the set node async
        /// </summary>
        /// <param name="indexname">The name of the new index</param>
        /// <returns>False either if the node is not set och if the index wasn't created, else it returns true</returns>
        public async Task<bool> CreateIndexAsync(string indexname)
        {
            if (!IsClientSet) return false;
            
            var settings = new IndexSettings();
            settings.NumberOfReplicas = 0;
            settings.NumberOfShards = 1;


            ElasticServerClient.CreateIndex(indexname, c => c
                .Index(indexname)
                .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0)));

            var exists = await IndexExistsAsync(indexname);
            return exists;
        }

        /// <summary>
        /// Indexes the sample to the specified index on the specified node.
        /// </summary>
        /// <param name="sample">The sample to be indexed</param>
        public void SendToIndex(PerformanceSample sample)
        {
            var result = ElasticServerClient.Index(sample);
            if (!result.IsValid)
            {
                File.WriteAllText(_debugLogPath, $"Sample not indexed: {sample}");
            }
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

        public async Task<bool> IndexExistsAsync(string indexname)
        {
            try
            {
                Func<bool> checkFunc = () => ElasticServerClient.IndexExists(indexname, i => i.Index(indexname)).Exists;
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

            //indexNameList.AddRange(indices.Records.Select(index => index.ToString()));

            foreach (var index in indices.Records)
            {
                indexNameList.Add(index.Index);
            }

            return indexNameList;
        }
    }
}
