using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using apack_v0_9.HelperClasses;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace apack_v0_9
{
    public class ReportsModel : ObservableObject
    {
        #region Construction

        public ReportsModel() 
        {
            this._rawData = new Dictionary<DateTime, List<double>>();
            this._dividedDataDict = new Dictionary<string, Dictionary<DateTime, double>>();
        }

        public ReportsModel(Dictionary<DateTime, List<double>> data, List<string> counterorder)
        {
            this._rawData = data;
            this._counterNamesAndOrder = counterorder;
            this.DividedDataDict = new Dictionary<string, Dictionary<DateTime, double>>();
            this._machineName = "Test machine";
            this._reportDescription = @"Habitasse vehicula nibh inceptos a bibendum porttitor luctus molestie, porttitor amet imperdiet justo magna quis eu tristique, taciti lectus vivamus lacinia nibh tellus eros at sodales tempus at primis mattis leo velit.";
            DivideData();
            CalculateAverages();
        }
        #endregion

        #region Members
        Dictionary<DateTime, List<double>> _rawData;
        List<string> _counterNamesAndOrder;
        Dictionary<string, Dictionary<DateTime, double>> _dividedDataDict;
        Dictionary<string, double> _averageValuesList;
        string _machineName;
        string _reportDescription;
        string _perfLogFilePath;

        #endregion

        #region Properties
        public Dictionary<DateTime, List<double>> RawData
        {
            get
            {
                return _rawData;
            }
            set
            {
                if (this._rawData != value)
                {
                    this._rawData = value;
                    RaisePropertyChanged("RawData");
                    DivideData();
                }
                
            }
        }

        public string PerfLogFilePath
        {
            get { return _perfLogFilePath; }
            set
            {
                if (_perfLogFilePath != value)
                {
                    _perfLogFilePath = value;
                    RaisePropertyChanged("PerfLogFilePath");
                }
            }
        }

        public List<string> CounterNamesAndOrder
        {
            get
            {
                return this._counterNamesAndOrder;
            }
            set
            {
                this._counterNamesAndOrder = value;
            }
        }

        public Dictionary<string, Dictionary<DateTime, double>> DividedDataDict
        {
            get
            {
                    return this._dividedDataDict;
            }
            set
            {
                this._dividedDataDict = value;
            }
        }

        public string ReportDescription
        {
            get { return this._reportDescription; }
            set 
            {

                this._reportDescription = value.Replace(Environment.NewLine, String.Empty).Replace("  ", String.Empty).Replace("    ",String.Empty); 
            }
        }

        public string MachineName
        {
            get { return this._machineName; }
            set { this._machineName = value; }
        }

        public Dictionary<string, double> AverageValuesList
        {
            get 
            {
                if (this.DividedDataDict.Count != 0)
                {
                    CalculateAverages();
                }

                return this._averageValuesList;
            }

            set { this._averageValuesList = value; }
        }

        public int NumberOfSamples
        {
            get { return this.RawData.Count; }
        }
        #endregion

        #region Methods
        private void DivideData() 
        {
            //Clear the property if it has items
            if (this.DividedDataDict.Count != 0)
            {
                this.DividedDataDict.Clear();
            }

            // Make sure that the CounterOrder property has items
            if (this.CounterNamesAndOrder != null && this.CounterNamesAndOrder.Count != 0)
            {
                for (int k = 1; k < this.CounterNamesAndOrder.Count; k++)
                {
                    Dictionary<DateTime, double> dict = new Dictionary<DateTime, double>();
                    this.DividedDataDict.Add(this.CounterNamesAndOrder[k], dict);
                }
            }
            else
            {
                throw new InvalidDataException();
            }

            for (int i = 0; i < this.CounterNamesAndOrder.Count - 1; i++)
            {
                for (int j = 0; j < this.RawData.Count; j++)
                {

                    DateTime date = this.RawData.ElementAt(j).Key;
                    double value = this.RawData.ElementAt(j).Value[i];

                    this.DividedDataDict.ElementAt(i).Value.Add(date, value);
                }
            }
        }

        private void CalculateAverages()
        {
            Dictionary<string, double> averagesList = new Dictionary<string, double>();
            int sampleNumberOf;
            double sampleSum;
            double sampleAverage;

            foreach (string str in DividedDataDict.Keys)
            {
                sampleNumberOf = 0;
                sampleSum = 0;
                sampleAverage = 0;
                Dictionary<DateTime, double> tempDict = this.DividedDataDict[str];
                foreach (double sample in tempDict.Values)
                {
                    sampleSum += sample;
                    sampleNumberOf++;
                }
                sampleAverage = sampleSum / sampleNumberOf;
                averagesList.Add(str, sampleAverage);
            }

            this.AverageValuesList = averagesList;
        }

        public void SaveCharts()
        {
            foreach(KeyValuePair<string, Dictionary<DateTime, double>> counter in this.DividedDataDict)
            {
                CreateChart(counter.Key, counter.Value);
            }
        }

        private void CreateChart(string counter, Dictionary<DateTime, double> dictionary)
        {
            // Create the chart
            var chart = new Chart();
            chart.Size = new Size(1200, 500);

            var chartArea = new ChartArea();
            chartArea.AxisX.LabelStyle.Format = "dd/MMM\nhh:mm";
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Consolas", 8);
            chart.ChartAreas.Add(chartArea);

            var series = new Series();
            series.Name = "Series1";
            series.ChartType = SeriesChartType.FastLine;
            series.XValueType = ChartValueType.DateTime;
            chart.Series.Add(series);

            // Bind the data points
            chart.Series["Series1"].Points.DataBindXY(dictionary.Keys, dictionary.Values);

            // Copy the series and manipulate the copy
            //chart.DataManipulator.CopySeriesValues("Series1", "Series2");
            //chart.DataManipulator.FinancialFormula(FinancialFormula.WeightedMovingAverage, "Series2");
            //chart.Series["Series2"].ChartType = SeriesChartType.FastLine;

            // Draw chart
            chart.Invalidate();

            string path = @"C:\\Users\\" + Environment.UserName + @"\Documents\output\" + counter + @".png";
            // Save image as file
            chart.SaveImage(path, ChartImageFormat.Png);
        }

        public void CreateReport()
        {
            string rootPath = @"C:\\Users\\" + Environment.UserName + @"\Documents\output\";
            FileStream fs = new FileStream(rootPath+ @"\Example_Report1.pdf", FileMode.Create, FileAccess.Write, FileShare.None);

            iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.A4);

            Document doc = new Document(iTextSharp.text.PageSize.A4, 36, 72, 108, 180);

            PdfWriter writer = PdfWriter.GetInstance(doc, fs);


            iTextSharp.text.Font title = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 28, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font heading1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font normal = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL);

            doc.Open();

            iTextSharp.text.Image axis_logo = iTextSharp.text.Image.GetInstance(rootPath + @"\logo_axis_color_rgb.png");
            axis_logo.ScalePercent(20);
            axis_logo.SetAbsolutePosition(5f, doc.PageSize.Height - 5f - 51f);

            doc.Add(axis_logo);

            

            doc.Add(new Paragraph("APACK Report", title));
            doc.Add(new Paragraph("Machine tested: " + this.MachineName, heading1));
            doc.Add(new Paragraph("Description: " + this.ReportDescription, normal));

            doc.Add(new Paragraph());

            doc.Add(new Paragraph("CPU", heading1));
            iTextSharp.text.Image cpu_image = iTextSharp.text.Image.GetInstance(rootPath + @"\cpu.png");
            cpu_image.ScalePercent(20);
            doc.Add(cpu_image);

            doc.Add(new Paragraph("Average Disk Queue Length", heading1));
            iTextSharp.text.Image disk_image = iTextSharp.text.Image.GetInstance(rootPath + @"\disk.png");
            disk_image.ScalePercent(20);
            doc.Add(disk_image);

            doc.Add(new Paragraph("Averages: ", heading1));
            foreach (KeyValuePair<string, double> pair in this.AverageValuesList)  //senaste nullreferenceexception
            {

                doc.Add(new Paragraph(pair.Key + " : " + pair.Value));
            }



            doc.Close();
        }

        public void ReadFromFile()
        {
            string file_path = @"C:\Users\" + Environment.UserName + @"\Documents\output\output.txt";
            DateTime current_date;
            Dictionary<DateTime, List<double>> perf_log = new Dictionary<DateTime, List<double>>();

            // Read all lines from the file
            string[] lines = File.ReadAllLines(this.PerfLogFilePath);

            // Read each line into the dictionary
            for ( int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    this.CounterNamesAndOrder = lines[i].Split(',').ToList<string>();
                }
                else
                {
                    List<double> current_perfvalues = new List<double>();
                    string[] values = lines[i].Split(',');
                    current_date = Convert.ToDateTime(values[0]);

                    for (int j = 1; j < values.Length; j++)
                    {
                        current_perfvalues.Add(Convert.ToDouble(values[j]));
                    }

                    perf_log[current_date] = current_perfvalues;
                }
               
            }

            this.RawData = perf_log;
        }

        #endregion
    }
}
