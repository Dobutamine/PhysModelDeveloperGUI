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
    /// Interaction logic for TrendGraph.xaml
    /// </summary>
    public partial class TrendGraph : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public float AutoScalingGridLines { get; set; } = 3f;

        private string parameterTitle = "";

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

        private int graphWidth = 1;

        public int GraphWidth
        {
            get { return graphWidth; }
            set
            {
                graphWidth = value;

            }
        }

        private string parameterUnit = "";

        public string ParameterUnit
        {
            get { return parameterUnit; }
            set { parameterUnit = value; OnPropertyChanged(); }
        }

        private string graphTitle = "";
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



        SKPoint[] parameter1Array;
        List<SKPoint> parameter1List = new List<SKPoint>();

        SKPoint[] parameter2Array;
        List<SKPoint> parameter2List = new List<SKPoint>();

        SKPoint[] parameter3Array;
        List<SKPoint> parameter3List = new List<SKPoint>();
        
        SKPoint[] parameter4Array;
        List<SKPoint> parameter4List = new List<SKPoint>();

        SKPoint[] parameter5Array;
        List<SKPoint> parameter5List = new List<SKPoint>();


        public SKPointMode PointMode1 { get; set; } = SKPointMode.Points;
        public SKPointMode PointMode2 { get; set; } = SKPointMode.Points;

        public bool AutoScale { get; set; } = false;
        public int AutoScaleSamples { get; set; } = 300;


        float pixelsPerUnitY = 1;
        float offsetY = 0;

        public bool GridXEnabled { get; set; } = false;
        public float GridXMin { get; set; } = 0;
        public float GridXMax { get; set; } = 100;
        public float GridXStep { get; set; } = 10;

        public bool GridYEnabled { get; set; } = false;
        public float GridYMin { get; set; } = 0;
        public float GridYMax { get; set; } = 200;
        public float GridYStep { get; set; } = 50;

        public SKPaint GraphPaint1 { get; set; }
        public SKPaint GraphPaint2 { get; set; }
        public SKPaint GraphPaint3 { get; set; }
        public SKPaint GraphPaint4 { get; set; }
        public SKPaint GraphPaint5 { get; set; }

        SKPaint GridPaint1 { get; set; }
        SKPaint GridText1 { get; set; }


        public string Legend1 = "HR";
        public string Legend2 = "SAT";
        public string Legend3 = "RESP";
        public string Legend4 = "SYST";
        public string Legend5 = "DIAST";

        SKPaint ErasePaint { get; set; }

        bool ClearFlag = true;

        int firstValueCounter = 0;

        bool refresh = true;

        float currentX = 0;
        public float xStepSize { get; set; } = 5;

        int MaxBufferSize { get; set; } = 1024;

        public TrendGraph()
        {
            InitializeComponent();

            DataContext = this;


            float[] dashArray = { 6, 6 };

            SKPathEffect dash = SKPathEffect.CreateDash(dashArray, 2);
            GridPaint1 = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 2,
                StrokeCap = SKStrokeCap.Round,
                PathEffect = dash,
                TextSize = 20f,
                IsAntialias = true
            };

            GridText1 = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                TextSize = 20f,
                IsAntialias = true
            };

            GraphPaint1 = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
               
                Color = SKColors.DarkGreen,
                StrokeWidth = 3,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };

            GraphPaint2 = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Fuchsia,
                StrokeWidth = 3,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),

                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };

            GraphPaint3 = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Black,
                StrokeWidth = 3,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),

                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };

            GraphPaint4 = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Red,
                StrokeWidth = 3,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),

                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };

            GraphPaint5 = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                Color = SKColors.Red,
                StrokeWidth = 3,
                TextSize = 20,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),

                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };


            ErasePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.White,
                IsAntialias = false
            };
        }


        private void GraphGrid_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            GridSurface = e.Surface;
            GridCanvas = GridSurface.Canvas;

            GridCanvas.Clear(SKColors.White);

            if (GridYEnabled)
            {
                // draw y grid lines
                pixelsPerUnitY = graphGrid.CanvasSize.Height / (GridYMax - GridYMin);
                float stepSizeY = GridYStep;

                float gridYText = GridYMax;


                for (float y = 0; y <= graphGrid.CanvasSize.Height; y += (stepSizeY * pixelsPerUnitY))
                {
                    GridCanvas.DrawLine(0, y, graphGrid.CanvasSize.Width, y, GridPaint1);

                
                    GridCanvas.DrawText(gridYText.ToString(), 5, y - 5, GridText1);
                    gridYText -= stepSizeY;

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


            }

            // rpint the legend
            float x1 = 300;
            float x2 = x1 + 120;
            float x3 = x2 + 120;
            float x4 = x3 + 120;
            float x5 = x4 + 120;

            float y1 = 30;

            GridCanvas.DrawText(Legend1, x1,  y1, GraphPaint1);
            GridCanvas.DrawText(Legend2, x2, y1, GraphPaint2);
            GridCanvas.DrawText(Legend3, x3, y1, GraphPaint3);
            GridCanvas.DrawText(Legend4, x4, y1, GraphPaint4);
            GridCanvas.DrawText(Legend5, x5, y1, GraphPaint5);




            if (!AutoScale) refresh = true;


        }

        private void GraphMain_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            MainSurface = e.Surface;
            MainCanvas = MainSurface.Canvas;

            if (refresh)
            {
                MainCanvas.Clear(SKColors.White);
                refresh = false;
            }
            // draw eraser
        
            parameter1Array = parameter1List.ToArray();
            MainCanvas.DrawPoints(PointMode1, parameter1Array, GraphPaint1);

            parameter2Array = parameter2List.ToArray();
            MainCanvas.DrawPoints(PointMode1, parameter2Array, GraphPaint2);

            parameter3Array = parameter3List.ToArray();
            MainCanvas.DrawPoints(PointMode1, parameter3Array, GraphPaint3);

            parameter4Array = parameter4List.ToArray();
            MainCanvas.DrawPoints(PointMode1, parameter4Array, GraphPaint4);

            parameter5Array = parameter5List.ToArray();
            MainCanvas.DrawPoints(PointMode1, parameter5Array, GraphPaint5);

            //MainCanvas.DrawRect(currentX + xStepSize, 0, 40, graphMain.CanvasSize.Height, ErasePaint);

            firstValueCounter++;

        }



        public void WriteBuffer(double d1, double d2, double d3, double d4, double d5)
        {
            SKPoint newPoint1 = new SKPoint()
            {
                X = currentX,
                Y = graphMain.CanvasSize.Height - ((float)d1 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY,
            };
            parameter1List.Add(newPoint1);

            SKPoint newPoint2 = new SKPoint()
            {
                X = currentX,
                Y = graphMain.CanvasSize.Height - ((float)d2 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY,
            };
            parameter2List.Add(newPoint2);

            SKPoint newPoint3 = new SKPoint()
            {
                X = currentX,
                Y = graphMain.CanvasSize.Height - ((float)d3 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY,
            };
            parameter3List.Add(newPoint3);

            SKPoint newPoint4 = new SKPoint()
            {
                X = currentX,
                Y = graphMain.CanvasSize.Height - ((float)d4 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY,
            };
            parameter4List.Add(newPoint4);

            SKPoint newPoint5 = new SKPoint()
            {
                X = currentX,
                Y = graphMain.CanvasSize.Height - ((float)d5 - GridYMin) * pixelsPerUnitY - offsetY * pixelsPerUnitY,
            };
            parameter5List.Add(newPoint5);

            currentX += xStepSize;

        }

        public void DrawGrid()
        {
            graphGrid.InvalidateVisual();

        }

        public void Draw()
        {
            // now draw this array
            graphMain.InvalidateVisual();
        }



    }
}

