using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PhysModelLibrary;
using SkiaSharp;

namespace PhysModelDeveloperGUI
{
    /// <summary>
    /// Interaction logic for PatientMonitor.xaml
    /// </summary>
    public partial class PatientMonitor : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool monitorVisible = true;

        private string param1Value;
        public string Param1Value
        {
            get { return param1Value; }
            set { param1Value = value; OnPropertyChanged(); }
        }
        private string param1Name;
        public string Param1Name
        {
            get { return param1Name; }
            set { param1Name = value; OnPropertyChanged(); }
        }
        private string param1Unit;
        public string Param1Unit
        {
            get { return param1Unit; }
            set { param1Unit = value; OnPropertyChanged(); }
        }

        private string param2Value;
        public string Param2Value
        {
            get { return param2Value; }
            set { param2Value = value; OnPropertyChanged(); }
        }
        private string param2Name;
        public string Param2Name
        {
            get { return param2Name; }
            set { param2Name = value; OnPropertyChanged(); }
        }
        private string param2Unit;
        public string Param2Unit
        {
            get { return param2Unit; }
            set { param2Unit = value; OnPropertyChanged(); }
        }

        private string param3Value;
        public string Param3Value
        {
            get { return param3Value; }
            set { param3Value = value; OnPropertyChanged(); }
        }
        private string param3Name;
        public string Param3Name
        {
            get { return param3Name; }
            set { param3Name = value; OnPropertyChanged(); }
        }
        private string param3Unit;
        public string Param3Unit
        {
            get { return param3Unit; }
            set { param3Unit = value; OnPropertyChanged(); }
        }

        private string param4Value;
        public string Param4Value
        {
            get { return param4Value; }
            set { param4Value = value; OnPropertyChanged(); }
        }
        private string param4Name;
        public string Param4Name
        {
            get { return param4Name; }
            set { param4Name = value; OnPropertyChanged(); }
        }
        private string param4Unit;
        public string Param4Unit
        {
            get { return param4Unit; }
            set { param4Unit = value; OnPropertyChanged(); }
        }

        private string param5Value;
        public string Param5Value
        {
            get { return param5Value; }
            set { param5Value = value; OnPropertyChanged(); }
        }
        private string param5Name;
        public string Param5Name
        {
            get { return param5Name; }
            set { param5Name = value; OnPropertyChanged(); }
        }
        private string param5Unit;
        public string Param5Unit
        {
            get { return param5Unit; }
            set { param5Unit = value; OnPropertyChanged(); }
        }

        public int GraphicsUpdateInterval { get; set; } = 30;

        public bool MonitorVisible
        {
            get { return monitorVisible; }
            set { monitorVisible = value; }
        }

        bool initialized = false;

        readonly DispatcherTimer updateTimer = new DispatcherTimer(DispatcherPriority.Render);

        public PatientMonitor()
        {
            InitializeComponent();

            DataContext = this;
        }

        public void UpdateCurves(double ecg, double abp, double spo2, double etco2, double resp)
        {
            if (initialized)
            {
                graph1.WriteBuffer(ecg);
                graph3.WriteBuffer(abp);
                graph2.WriteBuffer(spo2);
                graph4.WriteBuffer(etco2);
                graph5.WriteBuffer(resp);
            }
          
        }

        public void UpdateParameters(string p1, string p2, string p3, string p4, string p5)
        {
            graph1.ParameterValue = p1;
            graph2.ParameterValue = p2;
            graph3.ParameterValue = p3;
            graph4.ParameterValue = p4;
            graph5.ParameterValue = p5;
        }

        void DrawGraphs()
        {
            if (initialized && monitorVisible)
            {
                graph1.Draw();
                graph2.Draw();
                graph3.Draw();
                graph4.Draw();
                graph5.Draw();
            }
        }
        public void InitPatientMonitor()
        {
            InitGraph1();
            InitGraph2();
            InitGraph3();
            InitGraph4();
            InitGraph5();

            initialized = true;

            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, GraphicsUpdateInterval);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

        }


        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (initialized)
            {
                DrawGraphs();
            }
        }

        void InitGraph1()
        {
            graph1.GraphTitle = "ecg";
            graph1.ParameterTitle = "hr";
            graph1.ParameterUnit = "/min";
            graph1.GraphTitleColor = new SolidColorBrush(Colors.LimeGreen);
            graph1.GraphPaint1.Color = SKColors.LimeGreen;
            graph1.AutoScale = true;

        }

        void InitGraph3()
        {
            graph3.GraphTitle = "abp";
            graph3.ParameterTitle = "abp";
            graph3.ParameterUnit = "mmHg";
            graph3.GraphTitleColor = new SolidColorBrush(Colors.Red);
            graph3.GraphPaint1.Color = SKColors.Red;

            graph3.GridXEnabled = false;
            graph3.GridYEnabled = true;
            graph3.GridYMin = 20;
            graph3.GridYMax = 80;
            graph3.GridYStep = 20;

        }

        void InitGraph2()
        {
            graph2.GraphTitle = "spo2";
            graph2.ParameterTitle = "SpO2";
            graph2.ParameterUnit = "%";
            graph2.GraphTitleColor = new SolidColorBrush(Colors.Fuchsia);
            graph2.GraphPaint1.Color = SKColors.Fuchsia;
            graph2.AutoScale = true;

            graph2.GridXEnabled = false;
            graph2.GridYEnabled = true;
            graph2.GridYMin = 20;
            graph2.GridYMax = 80;
            graph2.GridYStep = 20;


        }

        void InitGraph4()
        {
            graph4.GraphTitle = "co2";
            graph4.ParameterTitle = "etCO2";
            graph4.ParameterUnit = "mmHg";
            graph4.GraphTitleColor = new SolidColorBrush(Colors.Yellow);
            graph4.GraphPaint1.Color = SKColors.Yellow;
            graph4.xStepSize = 2;

            graph4.GridXEnabled = false;
            graph4.GridYEnabled = true;
            graph4.GridYMin = 0;
            graph4.GridYMax = 80;
            graph4.GridYStep = 20;

        }
        void InitGraph5()
        {
            graph5.GraphTitle = "resp";
            graph5.ParameterTitle = "rf";
            graph5.ParameterUnit = "/min";
            graph5.GraphTitleColor = new SolidColorBrush(Colors.White);
            graph5.GraphPaint1.Color = SKColors.White;
            graph5.xStepSize = 2;
            graph5.AutoScale = true;


        }

    }
}
