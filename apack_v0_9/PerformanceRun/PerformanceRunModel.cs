using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using apack_v0_9.HelperClasses;
using Nest;


namespace apack_v0_9.PerformanceRun
{
    public class PerformanceRunModel : ObservableObject
    {
        #region Members

        //private string _name;
        //private string _description;
        string _lastPerfSample;
        bool _running;
        //string _fileName;
        #endregion

        #region Construction
        public PerformanceRunModel(string nodeaddress, string indexname, int runningtime)
        {
            Running = false;
            LastPerfSample = string.Empty;
            NodeAddress = nodeaddress;
            IndexName = indexname;
            RunningTime = runningtime;
        }
        #endregion

        #region Properties
        public int RunningTime { get; set; }

        string NodeAddress { get; set; }

        string IndexName { get; set; }
        
        public string LastPerfSample
        {
            get { return _lastPerfSample; }
            set 
            { 
                _lastPerfSample = value;
                RaisePropertyChanged("LastPerfSample");
            }
        }

        public bool Running
        {
            get
            {
                return _running;
            }
            set
            {
                _running = value;
                RaisePropertyChanged("Running");
            }
        }

        public int NumberOfCurrentSample { get; set; }

        public int NumberOfTotalSamples { get; set; }
        
        #endregion

        #region Methods

        static ManagementObjectSearcher GetSearcher(string query)
        {
            var mos = new ManagementObjectSearcher("root\\CIMV2", query);
            return mos;
        }

        public async Task<int> Run(CancellationToken cts)
        {
            Running = true;
            
            // Number of loops is the time in hours times 3600 (seconds in an hour)
            // divided by 15 since the delay between samples is 15 seconds
            var numberOfLoops = RunningTime * 3600 / 15;
            

            var objSearcherList = new List<ManagementObjectSearcher>();
            var perfSample = new PerformanceSample();

            // Add new WMI queries here
            objSearcherList.Add(GetSearcher("SELECT PercentProcessorTime, PercentInterruptTime, PercentPrivilegedTime FROM Win32_PerfFormattedData_PerfOSS_Processor WHERE Name = \"_Total\""));
            objSearcherList.Add(GetSearcher("SELECT AvgDiskQueueLength FROM  Win32_PerfFormattedData_PerfDisk_PhysicalDisk WHERE Name = \"_Total\""));

            for (var i = 0; i < numberOfLoops; i++)
            {
                NumberOfCurrentSample = i + 1;
                var currentTime = DateTime.Now;
                perfSample.SampleTime = currentTime;
                

                foreach (var searcher in objSearcherList)
                {
                    foreach (var o in searcher.Get())
                    {
                        var obj = (ManagementObject) o;
                        foreach (var prop in obj.Properties)
                        {
                            if (prop.Name == "PercentProcessorTime")
                            {
                                perfSample.Cpu = Convert.ToDouble(obj[prop.Name]);
                            }
                            else if (prop.Name == "AvgDiskQueueLength")
                            {
                                perfSample.AverageDiskQueueLength = Convert.ToDouble(obj[prop.Name]);
                            }
                            else if (prop.Name == "PercentInterruptTime")
                            {
                                perfSample.PercentInterruptTime = Convert.ToDouble(obj[prop.Name]);
                            }
                            else if (prop.Name == "PercentPrivilegedTime")
                            {
                                perfSample.PercentPrivilegedTime = Convert.ToDouble(obj[prop.Name]);
                            }
                        }
                    }
                }

                
                if (perfSample.Cpu != null)
                {
                    // Send to elastic node
                    SendToElasticSearch(perfSample, NodeAddress, IndexName);

                    // Set console message
                    LastPerfSample =
                        $"{currentTime} CPU Load: {perfSample.Cpu} %, AvgDiskQueueLength : '{perfSample.AverageDiskQueueLength}' Sample '{i}' of '{numberOfLoops}' ";
                }

                if (cts.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(3000, cts);
            }

            Running = false;
            return 1;   
        }

        static void SendToElasticSearch(PerformanceSample sample, string nodeaddress, string indexname)
        {
            var node = new Uri(nodeaddress);
            var config = new ConnectionSettings(node);
            config.DefaultIndex(indexname);
            var client = new ElasticClient(config);
          
            client.Index(sample);
        }

        public bool IndexExists(string indexname, string address)
        {
            try
            {
                var node = new Uri(address);
                var config = new ConnectionSettings(node);
                var client = new ElasticClient(config);

                var request = new IndexExistsRequest(indexname);
                var result = client.IndexExists(request);

                return result.Exists;
            }
            catch (UriFormatException)
            {
                //Handle format exception
                return false;
            }
        }

        
       
        #endregion
    }
}
