using PhysModelLibrary.Compartments;
using PhysModelLibrary.Connectors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysModelDeveloperGUI
{
    public class AnimatedGasConnector
    {

        public List<GasCompartmentConnector> connectors = new List<GasCompartmentConnector>();
        public GasCompartment sizeCompartment;


        SKPaint circleOut = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            Color = SKColors.DarkBlue,
            StrokeCap = SKStrokeCap.Square,
            StrokeWidth = 5,
        };

        SKPaint circleOrigen = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            Color = SKColors.Green,
            StrokeWidth = 5,
        };

        SKPaint circleTarget = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            Color = SKColors.Orange,
            StrokeWidth = 5,
        };

        SKColor colorFrom;
        SKColor colorTo;

        SKPaint paint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.AliceBlue,
            StrokeWidth = 10
        };

        SKPaint textPaint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Arial Bold"),
            FakeBoldText = true,
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            Color = SKColors.White,
            IsStroke = false,
            TextSize = 16f


        };
        public SKPoint locationOrigen = new SKPoint(0, 0);
        public SKPoint locationTarget = new SKPoint(0, 0);
        public SKPoint location1 = new SKPoint(0, 0);
        public SKPoint location2 = new SKPoint(0, 0);
        public SKPoint location3 = new SKPoint(0, 0);
        public SKPoint location4 = new SKPoint(0, 0);

        public float scaleRelative = 18;
        float scale = 1;
        public float Degrees { get; set; } = 0;
        public float RadiusXOffset { get; set; } = 1;
        public float RadiusYOffset { get; set; } = 1;

        float currentAngle = 0;
        public float StartAngle { get; set; } = 0;
        public float EndAngle { get; set; } = 0;
        public float Direction { get; set; } = 1;
        public float Speed { get; set; } = 0.05f;

        public float XOffset { get; set; } = 0;
        public float YOffset { get; set; } = 0;

        public int Mode { get; set; } = 0;
        public bool NoLoss { get; set; } = true;
        public string Name { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public string Title { get; set; } = "O2";


        public void AddConnector(GasCompartmentConnector c)
        {
            connectors.Add(c);
        }


        public void DrawConnector(SKCanvas canvas, float _radX, float _radY)
        {
            float totalFlow = 0;
            float totalSpO2 = 0;
            float totalSpO2To = 0;
            float totalSpO2From = 0;
            float currentVolume = 0;
            float radius = 0;

            scale = _radX * scaleRelative;
            radius = _radX / 2.5f;

            if (_radX > _radY)
            {
                scale = _radY * scaleRelative;
                radius = _radY / 2.5f;
            }

            // calculate position
            locationOrigen = GetPosition(StartAngle, radius);
            locationTarget = GetPosition(EndAngle, radius);

            float left = (float)Math.Sin(270 * 0.0174532925) * RadiusXOffset * radius + XOffset;
            float right = (float)Math.Sin(90 * 0.0174532925) * RadiusXOffset * radius + XOffset;
            float top = (float)Math.Cos(180 * 0.0174532925) * RadiusYOffset * radius + YOffset;
            float bottom = (float)Math.Cos(0 * 0.0174532925) * RadiusYOffset * radius + YOffset;

            // calculate the total volume and average spO2 if lumping is the case
            foreach (GasCompartmentConnector c in connectors)
            {
                totalFlow += (float)c.CurrentFlow * Speed;
                if (totalFlow >= 0)
                {
                    totalSpO2 += (float)c.Comp1.TO2;
                    if (NoLoss)
                    {
                        totalSpO2From += (float)c.Comp1.TO2;
                        totalSpO2To += (float)c.Comp1.TO2;
                    }
                    else
                    {
                        totalSpO2From += (float)c.Comp1.TO2;
                        totalSpO2To += (float)c.Comp2.TO2;
                    }
                }
                else
                {
                    totalSpO2 += (float)c.Comp2.TO2;
                    if (NoLoss)
                    {
                        totalSpO2From += (float)c.Comp2.TO2;
                        totalSpO2To += (float)c.Comp2.TO2;
                    }
                    else
                    {
                        totalSpO2From += (float)c.Comp2.TO2;
                        totalSpO2To += (float)c.Comp1.TO2;
                    }

                }
                Title = "";
            }



            //paint.Color = CalculateColor(totalSpO2 / connectors.Count);
            colorTo = CalculateColor(totalSpO2To / connectors.Count);
            colorFrom = CalculateColor(totalSpO2From / connectors.Count);

            currentAngle += totalFlow * Direction * 3;
            if (currentAngle > Math.Abs(StartAngle - EndAngle))
            {
                currentAngle = 0;
            }
            if (currentAngle < 0)
            {
                currentAngle = 0;
            }

            currentAngle = 0;

            if (sizeCompartment != null)
            {
                currentVolume = (float)sizeCompartment.VolCurrent;
                circleOut.StrokeWidth = RadiusCalculator(currentVolume, scale);
            }
            else
            {
                circleOut.StrokeWidth = 40;
            }

            SKRect mainRect = new SKRect(left, top, right, bottom);


            //circleOut.Shader = SKShader.CreateSweepGradient(
            //    new SKPoint(0f, 0f),
            //    new SKColor[] { colorFrom, colorTo },
            //    new float[] { StartAngle / 360f, EndAngle / 360f }
            //);

            using (SKPath path = new SKPath())
            {
                path.AddArc(mainRect, StartAngle, Math.Abs(StartAngle - EndAngle));
                canvas.DrawPath(path, circleOut);
            }

            location1 = GetPosition(StartAngle + currentAngle, radius);
            canvas.DrawCircle(location1.X + XOffset, location1.Y + YOffset, 10, paint);

        }



        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        SKColor CalculateColor(double sat)
        {
            // sat can variate between 0 - 10
            if (sat > 10)
            {
                sat = 10;
            }

            float remap = Remap((float)sat, 0, 10, -2f, 1);

            if (remap < 0) remap = 0;

            byte red = (byte)(remap * 250);
            byte green = (byte)(remap * 100);
            byte blue = (byte)(80 + remap * 75);

            return new SKColor(red, green, blue);
        }

        SKPoint GetPosition(float _degrees, float _rad)
        {
            SKPoint point = new SKPoint
            {
                X = (float)Math.Cos(_degrees * 0.0174532925) * RadiusXOffset * _rad,
                Y = (float)Math.Sin(_degrees * 0.0174532925) * RadiusYOffset * _rad
            };

            return point;
        }

        float RadiusCalculator(double vol, float scale)
        {
            float _radius, _cubicRadius;

            _cubicRadius = (float)(vol / ((4f / 3f) * Math.PI));
            _radius = (float)Math.Pow(_cubicRadius, 1f / 3f);

            return _radius * scale;

        }


    }
}
