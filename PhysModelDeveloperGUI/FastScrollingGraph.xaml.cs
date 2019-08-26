using SkiaSharp;
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

namespace PhysModelDeveloperGUI
{
    /// <summary>
    /// Interaction logic for PatientMonitor.xaml
    /// </summary>
    public partial class FastScrollingGraph : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string parameterTitle = "heartrate";

        public string ParameterTitle
        {
            get { return parameterTitle; }
            set { parameterTitle = value; OnPropertyChanged(); }
        }

        private string parameterValue;

        public string ParameterValue
        {
            get { return parameterValue; }
            set { parameterValue = value; OnPropertyChanged(); }
        }

        private string parameterUnit = "/min";

        public string ParameterUnit
        {
            get { return parameterUnit; }
            set { parameterUnit = value; OnPropertyChanged(); }
        }

        private string graphTitle = "ecg";
        public string GraphTitle
        {
            get { return graphTitle; }
            set { graphTitle = value; OnPropertyChanged(); }
        }

        private SolidColorBrush graphTitleColor = new SolidColorBrush(Colors.White);
        public SolidColorBrush GraphTitleColor
        {
            get { return graphTitleColor; }
            set { graphTitleColor = value; OnPropertyChanged(); }
        }

        private int fontSizeTitle = 10;

        public int FontSizeTitle
        {
            get { return fontSizeTitle; }
            set { fontSizeTitle = value; OnPropertyChanged(); }
        }

        private int fontSizeValue = 15;

        public int FontSizeValue
        {
            get { return fontSizeValue; }
            set { fontSizeValue = value; OnPropertyChanged(); }
        }


        SkiaSharp.SKSurface MainSurface { get; set; }
        SkiaSharp.SKCanvas MainCanvas { get; set; }

        SkiaSharp.SKSurface GridSurface { get; set; }
        SkiaSharp.SKCanvas GridCanvas { get; set; }

        List<double> DataBuffer1 { get; set; } = new List<double>();

        List<double> ExtractedBufferData1 { get; set; } = new List<double>();


        SKPoint[] displayArray1 = new SKPoint[1000];


        SKPointMode PointMode1 { get; set; } = SKPointMode.Polygon;
        SKPointMode PointMode2 { get; set; } = SKPointMode.Lines;

        public bool AutoScale { get; set; } = false;
        public int AutoScaleSamples { get; set; } = 300;
        int sampleCounter = 0;
        double maxSample = -100000000;
        double minSample = 100000000;

        float pixelsPerUnitY = 1;
        float offsetY = 0;
        float autoscaleOffset = 0.50f;

        public bool GridXEnabled { get; set; } = false;
        public float GridXMin { get; set; } = 0;
        public float GridXMax { get; set; } = 100;
        public float GridXStep { get; set; } = 10;

        public bool GridYEnabled { get; set; } = false;
        public float GridYMin { get; set; } = 0;
        public float GridYMax { get; set; } = 100;
        public float GridYStep { get; set; } = 10;

        public SKPaint GraphPaint1 { get; set; }

        SKPaint GridPaint1 { get; set; }

        SKPaint ErasePaint { get; set; }

        bool ClearFlag = true;

        bool refresh = true;

        float currentX = 0;
        public float xStepSize { get; set; } = 5;

        int MaxBufferSize { get; set; } = 1024;

        public FastScrollingGraph()
        {
            InitializeComponent();

            DataContext = this;


            float[] dashArray = { 6, 6 };

            SKPathEffect dash = SKPathEffect.CreateDash(dashArray, 2);
            GridPaint1 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.White,
                StrokeWidth = 4,
                StrokeCap = SKStrokeCap.Round,
                PathEffect = dash,
                IsAntialias = false
            };

            GraphPaint1 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.LimeGreen,
                StrokeWidth = 3,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };

            ErasePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black,
                IsAntialias = false
            };
        }

        private void GraphGrid_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            GridSurface = e.Surface;
            GridCanvas = GridSurface.Canvas;

            if (GridYEnabled)
            {
                // draw y grid lines
                pixelsPerUnitY = graphGrid.CanvasSize.Height / (GridYMax - GridYMin);
                float stepSizeY = GridYStep;

                for (float y = 0; y <= graphGrid.CanvasSize.Height; y += (stepSizeY * pixelsPerUnitY))
                {
                    GridCanvas.DrawLine(0, y, graphGrid.CanvasSize.Width, y, GridPaint1);
                }
            }

            if (GridXEnabled)
            { 
                // draw x grid lines
                float pixelsPerUnitX = graphGrid.CanvasSize.Width / (GridXMax - GridXMin);
                float stepSizeX = GridXStep;

                for (float x = 0; x <= graphGrid.CanvasSize.Width; x += (stepSizeX * pixelsPerUnitX))
                {
                    GridCanvas.DrawLine(x, 0, x, graphGrid.CanvasSize.Height, GridPaint1);
                }

                refresh = true;
            }

            FontSizeTitle = (int) graphGrid.CanvasSize.Height / 20;
            FontSizeValue = (int)graphGrid.CanvasSize.Height / 7;

        }

        private void GraphMain_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            MainSurface = e.Surface;
            MainCanvas = MainSurface.Canvas;

            if (refresh)
            {
                MainCanvas.Clear(SKColors.Black);
                refresh = false;
            }
            // draw eraser
            MainCanvas.DrawRect(currentX, 0, 60, graphMain.CanvasSize.Height, ErasePaint);

            if (ClearFlag)
            {
                //MainCanvas.Clear(SKColors.Transparent);
                ClearFlag = false;
                if (displayArray1 != null && displayArray1.Length > 0)
                {
                    MainCanvas.DrawPoints(PointMode2, displayArray1, GraphPaint1);
                }

            } else
            {
                if (displayArray1 != null && displayArray1.Length > 0)
                {
                    MainCanvas.DrawPoints(PointMode1, displayArray1, GraphPaint1);
                }


            }

           
        }

        void AutoScaling(double sample)
        {
            if (AutoScale)
            {
                if (sample > maxSample) maxSample = sample;
                if (sample < minSample) minSample = sample;

                if (sampleCounter > AutoScaleSamples)
                {
                    GridYMax = (float)maxSample + (float) maxSample * autoscaleOffset;
                    if (minSample > 0)
                    {
                        GridYMin = (float)minSample - (float)minSample * autoscaleOffset;
                    } else
                    {
                        GridYMin = (float)minSample + (float)minSample * autoscaleOffset;
                    }
                    
                    // get posible negative offset
                    if (GridYMin < 0)
                    {
                        offsetY = Math.Abs(GridYMin);
                    } else
                    {
                        offsetY = 0;
                    }
                    GridYMax += offsetY;
                    GridYMin += offsetY;

                    pixelsPerUnitY = graphGrid.CanvasSize.Height / (GridYMax - GridYMin);
                   

                    maxSample = -100000000;
                    minSample = 100000000;
                    sampleCounter = 0;

                }
                sampleCounter++;
            }
        }

        public void UpdateParameterValue(double p1, double p2 = 0, double p3 = 0)
        {
            if (p2 == 0)
            {
                ParameterValue = p1.ToString();
            } else
            {
                ParameterValue = p1.ToString() + "/" + p2.ToString();
            }
            

        }
        public void WriteBuffer(double d1)
        {
            lock (DataBuffer1)
            {
           

                DataBuffer1.Add(d1);

                if (DataBuffer1.Count > MaxBufferSize)
                {
                    DataBuffer1.RemoveAt(0);
                }

               
            }

        }

        public void DrawGrid()
        {
            graphGrid.InvalidateVisual();

        }
        public void Draw()
        {
            ReadBuffer();

            // process this DisplayData list
            ProcessReadBuffer();

        }
        void ReadBuffer()
        {
            lock (DataBuffer1)
            {
                // get the actual buffer size
                int actualBuffer = DataBuffer1.Count;

                if (actualBuffer > 0)
                {
                    // get the current buffer data
                    ExtractedBufferData1 = DataBuffer1.GetRange(0, actualBuffer);
                    DataBuffer1.Clear();                
                }     
            }

        }

        void ProcessReadBuffer()
        {
            Array.Resize(ref displayArray1, ExtractedBufferData1.Count);

            double lastY1 = 0;

            currentX -= xStepSize;
            
            if (currentX < 0) { currentX = 0; }
            // now we have to form a SKPoint array from this datalist
            //foreach (double y in ExtractedBufferData1)
            for(int arrayCounter = 0; arrayCounter < ExtractedBufferData1.Count; arrayCounter++)
            {
               

                lastY1 = ExtractedBufferData1[arrayCounter];

                AutoScaling(lastY1);

                displayArray1[arrayCounter].X = currentX;

                displayArray1[arrayCounter].Y = graphMain.CanvasSize.Height - ((float)lastY1 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY;



                currentX += xStepSize;
                if (currentX > graphMain.CanvasSize.Width)
                {
                    currentX = 0;          
                    ClearFlag = true;
                    break;
                }


             }

            // store the last coordinate
            DataBuffer1.Add(lastY1);

            // now draw this array
            graphMain.InvalidateVisual();

        }


    }
}
