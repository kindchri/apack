using System;
using Nest;

namespace apack.HelperClasses
{
    [ElasticsearchType(Name = "performance_samples", IdProperty = "SampleTime")]
    public class PerformanceSample
    {
        #region Construction
        public PerformanceSample() { }

        public PerformanceSample(DateTime sampletime, double cpu, double avgdiskqueuelength)
        {
            SampleTime = sampletime;
            PercentProcessorTime = cpu;
            AverageDiskQueueLength = avgdiskqueuelength;
        }
        #endregion

        #region Properties
        [Date]
        public DateTime SampleTime { get; set; }

        [Number]
        public double? PercentProcessorTime { get; set; }

        [Number]
        public double? AverageDiskQueueLength { get; set; }

        [Number]
        public double?  PercentInterruptTime { get; set; }

        [Number]
        public double? PercentPrivilegedTime { get; set; }

        [Number]
        public double? AvailableMBytes { get; set; }

        [Number]
        public double? PagesPersec { get; set; }

        [Number]
        public double? CacheBytes { get; set; }

        [Number]
        public double? PercentCommittedBytesInUse { get; set; }

        [Number]
        public double? FreeSystemPageTableEntries { get; set; }

        [Number]
        public double? PageReadsPersec { get; set; }

        [Number]
        public double? PoolPagedBytes { get; set; }

        [Number]
        public double? PoolNonpagedBytes { get; set; }

        [Number]
        public double? SystemCallsPersec { get; set; }

        [Number]
        public double? ContextSwitchesPersec { get; set; }

        [Number]
        public double? AvgDisksecPerWrite { get; set; }

        [Number]
        public double? AvgDisksecPerRead { get; set; }

        [Number]
        public double? PercentIdleTime { get; set; }

        [Number]
        public double? SplitIoPerSec { get; set; }

        [Number]
        public double? CurrentDiskQueueLength { get; set; }


        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Performance sample at {SampleTime}";
        }
        #endregion
    }
}
