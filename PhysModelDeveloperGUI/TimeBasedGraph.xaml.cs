using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace PhysModelDeveloperGUI
{
    /// <summary>
    /// Interaction logic for TimeBasedGraph.xaml
    /// </summary>
    public partial class TimeBasedGraph : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool data1Enabled = true;

        public bool Data1Enabled
        {
            get { return data1Enabled; }
            set { data1Enabled = value; OnPropertyChanged(); }
        }

        private bool data2Enabled = false;
        public bool Data2Enabled
        {
            get { return data2Enabled; }
            set { data2Enabled = value; OnPropertyChanged(); }
        }

        private bool data3Enabled = false;
        public bool Data3Enabled
        {
            get { return data3Enabled; }
            set { data3Enabled = value; OnPropertyChanged(); }
        }

        private bool data4Enabled = false;
        public bool Data4Enabled
        {
            get { return data4Enabled; }
            set { data4Enabled = value; OnPropertyChanged(); }
        }

        private bool data5Enabled = false;
        public bool Data5Enabled
        {
            get { return data5Enabled; }
            set { data5Enabled = value; OnPropertyChanged(); }
        }


        public ExtendedObservableCollection<ChartDataClass> Data1 { get; set; } = new ExtendedObservableCollection<ChartDataClass>();
        public ExtendedObservableCollection<ChartDataClass> Data2 { get; set; } = new ExtendedObservableCollection<ChartDataClass>();
        public ExtendedObservableCollection<ChartDataClass> Data3 { get; set; } = new ExtendedObservableCollection<ChartDataClass>();
        public ExtendedObservableCollection<ChartDataClass> Data4 { get; set; } = new ExtendedObservableCollection<ChartDataClass>();
        public ExtendedObservableCollection<ChartDataClass> Data5 { get; set; } = new ExtendedObservableCollection<ChartDataClass>();

        public ChartDataClass currentData1 = new ChartDataClass();
        public ChartDataClass currentData2 = new ChartDataClass();
        public ChartDataClass currentData3 = new ChartDataClass();
        public ChartDataClass currentData4 = new ChartDataClass();
        public ChartDataClass currentData5 = new ChartDataClass();

        private SolidColorBrush series1Color = new SolidColorBrush(Colors.DarkGreen);
        public SolidColorBrush Series1Color
        {
            get { return series1Color; }
            set { series1Color = value; OnPropertyChanged(); }
        }

        private SolidColorBrush series2Color = new SolidColorBrush(Colors.Fuchsia);
        public SolidColorBrush Series2Color
        {
            get { return series2Color; }
            set { series2Color = value; OnPropertyChanged(); }
        }

        private SolidColorBrush series3Color = new SolidColorBrush(Colors.Red);
        public SolidColorBrush Series3Color
        {
            get { return series3Color; }
            set { series3Color = value; OnPropertyChanged(); }
        }

        private SolidColorBrush series4Color = new SolidColorBrush(Colors.Red);
        public SolidColorBrush Series4Color
        {
            get { return series4Color; }
            set { series4Color = value; OnPropertyChanged(); }
        }

        private SolidColorBrush series5Color = new SolidColorBrush(Colors.Black);
        public SolidColorBrush Series5Color
        {
            get { return series5Color; }
            set { series5Color = value; OnPropertyChanged(); }
        }

        private string series1Legend = "heartrate";
        public string Series1Legend
        {
            get { return series1Legend; }
            set { series1Legend = value; OnPropertyChanged(); }
        }

        private string series2Legend = "o2 sat";
        public string Series2Legend
        {
            get { return series2Legend; }
            set { series2Legend = value; OnPropertyChanged(); }
        }

        private string series3Legend = "systole";
        public string Series3Legend
        {
            get { return series3Legend; }
            set { series3Legend = value; OnPropertyChanged(); }
        }

        private string series4Legend = "diastole";
        public string Series4Legend
        {
            get { return series4Legend; }
            set { series4Legend = value; OnPropertyChanged(); }
        }

        private string series5Legend = "resp rate";
        public string Series5Legend
        {
            get { return series5Legend; }
            set { series5Legend = value; OnPropertyChanged(); }
        }

        private int series1StrokeThickness = 1;
        public int Series1StrokeThickness
        {
            get { return series1StrokeThickness; }
            set { series1StrokeThickness = value; OnPropertyChanged(); }
        }

        private int series2StrokeThickness = 1;
        public int Series2StrokeThickness
        {
            get { return series2StrokeThickness; }
            set { series2StrokeThickness = value; OnPropertyChanged(); }
        }

        private int series3StrokeThickness = 1;
        public int Series3StrokeThickness
        {
            get { return series3StrokeThickness; }
            set { series3StrokeThickness = value; OnPropertyChanged(); }
        }

        private int series4StrokeThickness = 1;
        public int Series4StrokeThickness
        {
            get { return series4StrokeThickness; }
            set { series4StrokeThickness = value; OnPropertyChanged(); }
        }

        private int series5StrokeThickness = 1;
        public int Series5StrokeThickness
        {
            get { return series5StrokeThickness; }
            set { series5StrokeThickness = value; OnPropertyChanged(); }
        }

        private double _maxY = 250;
        public double MaxY
        {
            get => _maxY;
            set { _maxY = value; OnPropertyChanged(); }
        }

        private double _minY = 0;
        public double MinY
        {
            get => _minY;
            set { _minY = value; OnPropertyChanged(); }
        }

        private double _gridXStep = 10;
        public double GridXStep
        {
            get => _gridXStep;
            set { _gridXStep = value; OnPropertyChanged(); }
        }
        private double _gridYStep = 10;
        public double GridYStep
        {
            get => _gridYStep;
            set { _gridYStep = value; OnPropertyChanged(); }
        }

        private DateTime _minDate = DateTime.Now;
        public DateTime MinDate
        {
            get => _minDate;
            set { _minDate = value; OnPropertyChanged(); }
        }

        private DateTime _maxDate = DateTime.Now;
        public DateTime MaxDate
        {
            get => _maxDate;
            set { _maxDate = value; OnPropertyChanged(); }
        }

        DateTime startDate = DateTime.Now;
        public int GraphDuration { get; set; } = 10;            // how many seconds are displayed in seconds
        public int BufferSize { get; set; } = 20;           // determine the buffer size in seconds

        private bool _showXLabels = false;
        public bool ShowXLabels
        {
            get => _showXLabels;
            set { _showXLabels = value; OnPropertyChanged(); }
        }

        private bool _showYLabels = true;
        public bool ShowYLabels
        {
            get => _showYLabels;
            set { _showYLabels = value; OnPropertyChanged(); }
        }

        private bool firstRunFlag = true;
        private double runDuration = 0;

        public TimeBasedGraph()
        {
            InitializeComponent();

            DataContext = this;

            InitGraph(120, 180);

        }

        public void InitGraph(int duration, int buffer)
        {
            GraphDuration = duration;
            BufferSize = buffer;

            startDate = DateTime.Now;
            MinDate = DateTime.Now;
            MaxDate = MinDate.AddSeconds(GraphDuration);

            runDuration = 0;
            firstRunFlag = true;

            // attach to the graph
            dataSeries1.ItemsSource = Data1;
            dataSeries2.ItemsSource = Data2;
            dataSeries3.ItemsSource = Data3;
            dataSeries4.ItemsSource = Data4;
            dataSeries5.ItemsSource = Data5;
        }

        public void DrawData()
        {
            if (Data1Enabled)
            {
                Data1.BatchChanged();
            }

            if (Data2Enabled)
            {
                Data2.BatchChanged();
            }
            if (Data3Enabled)
            {
                Data3.BatchChanged();
            }
            if (Data4Enabled)
            {
                Data4.BatchChanged();
            }
            if (Data5Enabled)
            {
                Data5.BatchChanged();
            }

        }
        public void UpdateData(double d1, double d2, double d3 = 0, double d4 = 0, double d5 = 0)
        {
            // when the graph starts we need to set the start conditions
            if (firstRunFlag)
            {
                firstRunFlag = false;
                runDuration = 0;
                startDate = DateTime.Now;
                MinDate = DateTime.Now;
                MaxDate = MinDate.AddSeconds(GraphDuration);
            }

            // calculate the duration of the graph run
            runDuration = (DateTime.Now - startDate).TotalSeconds;

            // if the graph duration is longer than the interval we need to shift the range which is displayed
            if (runDuration > GraphDuration)
            {
                MaxDate = DateTime.Now;
                MinDate = MaxDate.AddSeconds(-GraphDuration);
            }

           
            // update the data
            if (Data1Enabled)
            {
                currentData1 = new ChartDataClass();
                currentData1.TimeValue = DateTime.Now;
                currentData1.YValue = d1;
                lock (Data1)
                {
                    Data1.ExecuteWithoutNotifying(items => { items.Add(currentData1); });
                    if (runDuration > BufferSize) { Data1.ExecuteWithoutNotifying(items => { items.RemoveAt(0); }); }

                }
            }
   
            if (Data2Enabled)
            {
                currentData2 = new ChartDataClass();
                currentData2.TimeValue = DateTime.Now;
                currentData2.YValue = d2;
                lock (Data2)
                {
                    Data2.ExecuteWithoutNotifying(items => { items.Add(currentData2); });
                    if (runDuration > BufferSize) { Data2.ExecuteWithoutNotifying(items => { items.RemoveAt(0); }); }

                }
            }

            
            if (Data3Enabled)
            {
                currentData3 = new ChartDataClass();
                currentData3.TimeValue = DateTime.Now;
                currentData3.YValue = d3;
                lock (Data3)
                {
                    Data3.ExecuteWithoutNotifying(items => { items.Add(currentData3); });
                    if (runDuration > BufferSize) { Data3.ExecuteWithoutNotifying(items => { items.RemoveAt(0); }); }

                }
            }

         
            if (Data4Enabled)
            {
                currentData4 = new ChartDataClass();
                currentData4.TimeValue = DateTime.Now;
                currentData4.YValue = d4;
                lock (Data4)
                {
                    Data4.ExecuteWithoutNotifying(items => { items.Add(currentData4); });
                    if (runDuration > BufferSize) { Data4.ExecuteWithoutNotifying(items => { items.RemoveAt(0); }); }

                }
            }

           
            if (Data5Enabled)
            {
                currentData5 = new ChartDataClass();
                currentData5.TimeValue = DateTime.Now;
                currentData5.YValue = d5;
                lock (Data5)
                {
                    Data5.ExecuteWithoutNotifying(items => { items.Add(currentData5); });
                    if (runDuration > BufferSize) { Data5.ExecuteWithoutNotifying(items => { items.RemoveAt(0); }); }

                }
            }
        }


        public class ChartDataClass
        {
            public DateTime TimeValue { get; set; }
            public double XValue { get; set; }
            public double YValue { get; set; }
        }

        public class ExtendedObservableCollection<T> : ObservableCollection<T>
        {
            public ExtendedObservableCollection()
            {
            }

            public ExtendedObservableCollection(IEnumerable<T> items)
                : base(items)
            {
            }

            public void BatchChanged()
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            public void ExecuteWithoutNotifying(Action<IList<T>> itemsAction)
            {
                itemsAction(Items);
            }
        }



    }
}
