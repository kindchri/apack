using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Management;
using System.Windows;
using apack.HelperClasses;



namespace apack.PerformanceRun
{
    public class PerformanceRunModel : ObservableObject
    {
        #region Members

        public static readonly PerformanceRunModel Instance = new PerformanceRunModel();
        string _lastPerfSample;
        bool _running;

        #endregion

        #region Construction
        PerformanceRunModel()
        {
            Running = false;
            LastPerfSample = string.Empty;
        }
        public PerformanceRunModel(string nodeaddress, string indexname, int runningtime)
        {
            Running = false;
            LastPerfSample = string.Empty;
            RunningTime = runningtime;
        }
        #endregion

        #region Properties
        public int RunningTime { get; set; }
        
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

        /// <summary>
        /// Collects and sends performance data to your selected node and index.
        /// </summary>
        /// <param name="cts">Cancellation token</param>
        /// <returns>0 if the collector ran the full time, 
        /// TODO: 1 if the node/index was not found, 
        /// 2 if the collection was cancelled.</returns>
        public async Task<int> Run(CancellationToken cts)
        {
            try
            {
                Running = true;
            
                // Number of loops is the time in hours times 3600 (seconds in an hour)
                // divided by 15 since the delay between samples is 15 seconds
                var numberOfLoops = RunningTime * 3600 / 15;
            

                var objSearcherList = new List<ManagementObjectSearcher>();
                var perfSample = new PerformanceSample();

                // Add new WMI queries here
                #region ManagementObjectSearcher list creation
                objSearcherList.Add(
                        GetSearcher(
                            "SELECT PercentProcessorTime, PercentInterruptTime, PercentPrivilegedTime FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name = \"_Total\""));
                    objSearcherList.Add(
                        GetSearcher(
                            "SELECT AvgDiskQueueLength, AvgDisksecPerWrite, AvgDisksecPerRead, PercentIdleTime, SplitIOPerSec, CurrentDiskQueueLength FROM  Win32_PerfFormattedData_PerfDisk_PhysicalDisk WHERE Name = \"_Total\""));
                    objSearcherList.Add(
                        GetSearcher(
                            "SELECT AvailableMBytes, PageReadsPersec, PagesPersec, CacheBytes, PercentCommittedBytesInUse, FreeSystemPageTableEntries, PoolPagedBytes, PoolNonpagedBytes FROM Win32_PerfFormattedData_PerfOS_Memory"));
                    objSearcherList.Add(
                        GetSearcher(
                            "SELECT SystemCallsPersec, ContextSwitchesPersec FROM Win32_PerfFormattedData_PerfOS_System"));
                #endregion

                #region Collection Loop
                for (var i = 0; i <= numberOfLoops; i++)
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
                                    switch (prop.Name)
                                    {
                                        case "PercentProcessorTime":
                                            perfSample.PercentProcessorTime = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PercentInterruptTime":
                                            perfSample.PercentInterruptTime = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PercentPrivilegedTime":
                                            perfSample.PercentPrivilegedTime = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "AvgDiskQueueLength":
                                            perfSample.AverageDiskQueueLength = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "CurrentDiskQueueLength":
                                            perfSample.CurrentDiskQueueLength = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "AvgDisksecPerWrite":
                                            perfSample.AvgDisksecPerWrite = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "AvgDisksecPerRead":
                                            perfSample.AvgDisksecPerRead = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PercentIdleTime":
                                            perfSample.PercentIdleTime = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "SplitIOPerSec":
                                            perfSample.SplitIoPerSec = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "AvailableMBytes":
                                            perfSample.AvailableMBytes = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PageReadsPersec":
                                            perfSample.PageReadsPersec = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PagesPersec":
                                            perfSample.PagesPersec = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "CacheBytes":
                                            perfSample.CacheBytes = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PercentCommittedBytesInUse":
                                            perfSample.PercentCommittedBytesInUse = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "FreeSystemPageTableEntries":
                                            perfSample.FreeSystemPageTableEntries = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PoolPagedBytes":
                                            perfSample.PoolPagedBytes = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "PoolNonpagedBytes":
                                            perfSample.PoolNonpagedBytes = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "SystemCallsPersec":
                                            perfSample.SystemCallsPersec = Convert.ToDouble(obj[prop.Name]);
                                            break;

                                        case "ContextSwitchesPersec":
                                            perfSample.ContextSwitchesPersec = Convert.ToDouble(obj[prop.Name]);
                                            break;
                                    }
                                }
                            }

                            searcher.Dispose();
                        }



                        if (perfSample.PercentProcessorTime != null)
                        {
                            // Send to elastic node
                            ElasticServer.Instance.SendToIndex(perfSample);


                            // Set console message
                            LastPerfSample =
                                $"{currentTime} - Sample '{i}' of '{numberOfLoops}' ";
                        }

                    if (cts.IsCancellationRequested)
                    {
                        return 2;
                    }

                    await Task.Delay(15000, cts);

                    if (cts.IsCancellationRequested)
                    {
                        return 2;
                    }

                }
                #endregion

                return 0;
                }
            catch(ManagementException ex)
            {
                MessageBox.Show($"Exception \n {ex} \n was thrown.");
                return 1;
            }
            finally
            {
                Running = false;
            }
        }
        #endregion
    }
}
