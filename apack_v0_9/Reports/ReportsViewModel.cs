using System;
using System.Windows.Input;
using System.Windows;
using apack_v0_9.HelperClasses;

namespace apack_v0_9
{
    public class ReportsViewModel : ObservableObject, IPageViewModel
    {
        ReportsModel _report = new ReportsModel();

        public string Name => "Reports";

        public ReportsModel Report
        {
            get { return _report; }
            set
            {
                this._report = value;
                RaisePropertyChanged("ReportsModel");
            }
        }

        #region Methods
        void ReadFromFileExecute()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".txt";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                this.Report.PerfLogFilePath = dlg.FileName;
            }

            this.Report.ReadFromFile();

            //_performanceRunResult = new ReportsModel(this.Run.PerfData, new List<string> { "cpu", "disk" });
            MessageBox.Show(Report.NumberOfSamples + " samples read.");
        }

        bool CanReadFromFileExecute()
        {
            return true;
        }

        public ICommand ReadFromFile
        {
            get
            {
                return new RelayCommand(param => ReadFromFileExecute(), param => CanReadFromFileExecute());
            }
        }

        void SavePerfChartsExecute()
        {
            Report.SaveCharts();
        }

        bool CanSavePerfChartsExecute()
        {
            if (Report.RawData.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ICommand SavePerfCharts
        {
            get
            {
                return new RelayCommand(param => SavePerfChartsExecute(), param => CanSavePerfChartsExecute());
            }
        }

        void CreateReportExecute()
        {
            Report.CreateReport();
        }

        bool CanCreateReportExecute()
        {
            if (Report.NumberOfSamples != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ICommand CreateReport
        {
            get
            {
                return new RelayCommand(param => CreateReportExecute(), param => CanCreateReportExecute());
            }
        }
        #endregion
    }
}
