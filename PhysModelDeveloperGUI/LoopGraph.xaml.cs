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
    /// Interaction logic for LoopGraph.xaml
    /// </summary>
    public partial class LoopGraph : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        private int fontSizeValue = 25;

        public int FontSizeValue
        {
            get { return fontSizeValue; }
            set { fontSizeValue = value; OnPropertyChanged(); }
        }


        SkiaSharp.SKSurface MainSurface { get; set; }
        SkiaSharp.SKCanvas MainCanvas { get; set; }

        SkiaSharp.SKSurface GridSurface { get; set; }
        SkiaSharp.SKCanvas GridCanvas { get; set; }

        double[] DataBufferYArray;

        double[] DataBufferXArray;

        double[] ExtractedDataBufferYArray;

        double[] ExtractedDataBufferXArray;

        int arrayPositionY = 0;
        int arrayPositionX = 0;

        SKPoint[] displayArray1 = new SKPoint[1000];

        SKPointMode PointMode1 { get; set; } = SKPointMode.Points;

        public bool AutoScale { get; set; } = false;
        public int AutoScaleSamples { get; set; } = 300;
        int sampleCounter = 0;
        double maxSample = -100000000;
        double minSample = 100000000;

        float pixelsPerUnitY = 1;
        float offsetY = 0;
        float pixelsPerUnitX = 1;
        float offsetX = 0;
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

        public int ClearRefreshRate { get; set; } = 5;
        int clearRefreshCounter = 0;
        bool ClearFlag = true;

        bool refresh = true;


        public float xStepSize { get; set; } = 5;

        int MaxBufferSize { get; set; } = 5000;

        public LoopGraph()
        {
            InitializeComponent();

            DataContext = this;


            float[] dashArray = { 6, 6 };

            SKPathEffect dash = SKPathEffect.CreateDash(dashArray, 2);
            GridPaint1 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.DarkGray,
                StrokeWidth = 1,
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

            DataBufferYArray = new double[MaxBufferSize];
            DataBufferXArray = new double[MaxBufferSize];
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

                for (float y = (stepSizeY * pixelsPerUnitY); y < graphGrid.CanvasSize.Height; y += (stepSizeY * pixelsPerUnitY))
                {
                    GridCanvas.DrawLine(0, y, graphGrid.CanvasSize.Width, y, GridPaint1);
                }
            }

            if (GridXEnabled)
            {
                // draw x grid lines
                pixelsPerUnitX = graphGrid.CanvasSize.Width / (GridXMax - GridXMin);
                float stepSizeX = GridXStep;

                for (float x = (stepSizeX * pixelsPerUnitX); x < graphGrid.CanvasSize.Width; x += (stepSizeX * pixelsPerUnitX))
                {
                    GridCanvas.DrawLine(x, 0, x, graphGrid.CanvasSize.Height, GridPaint1);
                }
            }

            GridCanvas.DrawRect(0, 0, graphGrid.CanvasSize.Width - 2, graphGrid.CanvasSize.Height - 2, GridPaint1);
            refresh = true;


        }

        private void GraphMain_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            MainSurface = e.Surface;
            MainCanvas = MainSurface.Canvas;

            if (clearRefreshCounter > ClearRefreshRate)
            {
                clearRefreshCounter = 0;
                MainCanvas.Clear(SKColors.Transparent);
            }

            clearRefreshCounter++;


            if (displayArray1 != null && displayArray1.Length > 0)
            {
                MainCanvas.DrawPoints(PointMode1, displayArray1, GraphPaint1);
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
                    GridYMax = (float)maxSample + (float)maxSample * autoscaleOffset;
                    if (minSample > 0)
                    {
                        GridYMin = (float)minSample - (float)minSample * autoscaleOffset;
                    }
                    else
                    {
                        GridYMin = (float)minSample + (float)minSample * autoscaleOffset;
                    }

                    // get posible negative offset
                    if (GridYMin < 0)
                    {
                        offsetY = Math.Abs(GridYMin);
                    }
                    else
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

        public void WriteListToBuffer(double[] d1, double[] d2)
        {
            if ((DataBufferYArray.Length - arrayPositionY) < d1.Length)
            {
                arrayPositionY = 0;   
            }
            Array.Copy(d1, 0, DataBufferYArray, arrayPositionY, d1.Length);
            arrayPositionY += d1.Length;

            if ((DataBufferXArray.Length - arrayPositionX) < d2.Length)
            {
                arrayPositionX = 0;
            }
            Array.Copy(d2, 0, DataBufferXArray, arrayPositionX, d2.Length);
            arrayPositionX += d2.Length;
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
            Console.WriteLine("buffer size = " + arrayPositionY);
            Array.Resize(ref ExtractedDataBufferYArray, arrayPositionY);
            Array.Copy(DataBufferYArray, 0, ExtractedDataBufferYArray, 0, arrayPositionY);
            arrayPositionY = 0;

            Array.Resize(ref ExtractedDataBufferXArray, arrayPositionX);
            Array.Copy(DataBufferXArray, 0, ExtractedDataBufferXArray, 0, arrayPositionX);
            arrayPositionX = 0;

        }

        void ProcessReadBuffer()
        {
            try
            {

                int arraySize = 0;
                // find smallest array
                if (ExtractedDataBufferXArray.Length < ExtractedDataBufferYArray.Length)
                {
                    arraySize = ExtractedDataBufferXArray.Length;
                } else
                {
                    arraySize = ExtractedDataBufferYArray.Length;
                }

                // determine a display array
                Array.Resize(ref displayArray1, arraySize);

                double lastY1 = 0;
                double lastX1 = 0;

                // now we have to form a SKPoint array from this datalist
                //foreach (double y in ExtractedBufferData1)
                for (int arrayCounter = 0; arrayCounter < arraySize; arrayCounter++)
                {


                    lastY1 = ExtractedDataBufferYArray[arrayCounter];
                    lastX1 = ExtractedDataBufferXArray[arrayCounter];

                    AutoScaling(lastY1);

                    pixelsPerUnitY = graphGrid.CanvasSize.Height / (GridYMax - GridYMin);
                    pixelsPerUnitX = graphGrid.CanvasSize.Width / (GridXMax - GridXMin);

                    displayArray1[arrayCounter].X = ((float)lastX1 - GridXMin) * pixelsPerUnitX + offsetX * pixelsPerUnitX;
                    displayArray1[arrayCounter].Y = graphMain.CanvasSize.Height - ((float)lastY1 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY;

                }


                refresh = true;

                // now draw this array
                graphMain.InvalidateVisual();
            }
            catch { }

        }


    }
}
