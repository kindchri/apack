using System;
using Nest;

namespace apack_v0_9.PerformanceRun
{
    [ElasticsearchType(Name = "performance_samples", IdProperty = "SampleTime")]
    public class PerformanceSample
    {
        #region Construction
        public PerformanceSample() { }

        public PerformanceSample(DateTime sampletime, double cpu, double avgdiskqueuelength)
        {
            SampleTime = sampletime;
            Cpu = cpu;
            AverageDiskQueueLength = avgdiskqueuelength;
        }
        #endregion

        #region Properties
        [Date]
        public DateTime SampleTime { get; set; }

        [Number]
        public double? Cpu { get; set; }

        [Number]
        public double? AverageDiskQueueLength { get; set; }

        [Number]
        public double? PercentInterruptTime { get; set; }

        [Number]
        public double? PercentPrivilegedTime { get; set; }

        #endregion
    }
}
