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
    public class AnimatedShunt
    {
        public List<BloodCompartmentConnector> connectors = new List<BloodCompartmentConnector>();
        public BloodCompartment sizeCompartment;

        SKPaint textPaint2;

        SKPoint offset;

        SKPaint circleOut;

        SKPaint circleOrigen;

        SKPaint circleTarget;

        SKColor colorFrom;
        SKColor colorTo;

        SKPaint paint;
        SKPaint textPaint;

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

        public float currentAngle = 0;
        public float StartAngle { get; set; } = 0;
        public float EndAngle { get; set; } = 0;
        public float Direction { get; set; } = 1;
        public float Speed { get; set; } = 0.05f;
        public bool IsVisible { get; set; } = true;

        public float XOffset { get; set; } = 0;
        public float YOffset { get; set; } = 0;
        public float YOffsetShape { get; set; } = 0;

        public int Mode { get; set; } = 0;
        public bool NoLoss { get; set; } = false;
        public string Name { get; set; } = "";
        public string Title { get; set; } = "O2";

        public float Width { get; set; } = 60;
        float StrokeWidth = 15;

        public float AverageFlow { get; set; } = 0;
        int averageCounter = 0;
        float tempAverageFlow = 0;

        float currentStrokeWidth = 0;
        float strokeStepsize = 0.1f;

        float dpi = 1f;

        public AnimatedShunt(float _dpi)
        {
            dpi = (float)_dpi;

            textPaint2 = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Arial Bold"),
                Style = SKPaintStyle.Fill,
                FakeBoldText = true,
                IsAntialias = true,
                Color = SKColors.White,
                IsStroke = false,
                TextSize = 20f / dpi
            };

            offset = new SKPoint
            {
                X = 80,
                Y = 6

            };

            circleOut = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                Color = SKColors.Orange,
                StrokeWidth = 5,


            };

            circleOrigen = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                Color = SKColors.Green,
                StrokeWidth = 5,
            };

            circleTarget = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                Color = SKColors.Orange,
                StrokeWidth = 5,
            };

            paint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.BlanchedAlmond,
                StrokeWidth = 10
            };

            textPaint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Arial Bold"),
                FakeBoldText = true,
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
                Color = SKColors.Black,
                IsStroke = false,
                TextSize = 12f / dpi


            };

            Width /= dpi * 1.5f;
        }
        public void AddConnector(BloodCompartmentConnector c)
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


            scale = _radX * scaleRelative / dpi;
            radius = _radX / 2.5f;

            if (_radX > _radY)
            {
                scale = _radY * scaleRelative / dpi;
                radius = _radY / 2.5f;
            }

            float left = (float)Math.Sin(270 * 0.0174532925) * RadiusXOffset * radius + XOffset;
            float right = (float)Math.Sin(90 * 0.0174532925) * RadiusXOffset * radius + XOffset;
            float top = (float)Math.Cos(180 * 0.0174532925) * RadiusYOffset * radius + YOffset;
            float bottom = (float)Math.Cos(0 * 0.0174532925) * RadiusYOffset * radius + YOffset;

            // calculate the total volume and average spO2 if lumping is the case
            foreach (BloodCompartmentConnector c in connectors)
            {
                totalFlow += (float)c.RealFlow * Speed;
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

            tempAverageFlow += totalFlow;

            if (averageCounter > 100)
            {
                AverageFlow = Math.Abs(tempAverageFlow) / averageCounter;
                tempAverageFlow = 0;
                averageCounter = 0;
            }
            averageCounter++;


            //paint.Color = CalculateColor(totalSpO2 / connectors.Count);
            colorTo = CalculateColor(totalSpO2To / connectors.Count);
            colorFrom = CalculateColor(totalSpO2From / connectors.Count);

            currentAngle += totalFlow * Direction;

            // calculate position
            locationOrigen = GetPosition(StartAngle, radius);
            locationTarget = GetPosition(EndAngle, radius);

            locationOrigen.Y += YOffsetShape;
            locationTarget.Y += YOffsetShape;

            float dx = (locationOrigen.X - locationTarget.X) / Math.Abs(StartAngle - EndAngle);
            float dy = (locationOrigen.Y - locationTarget.Y) / Math.Abs(StartAngle - EndAngle);

            
            if (Math.Abs(currentAngle) > Math.Abs(StartAngle - EndAngle))
            {
                currentAngle = 0;
            }

            if (currentAngle < 0)
            {
                currentAngle = Math.Abs(StartAngle - EndAngle);
            }

            if (sizeCompartment != null)
            {
                currentVolume = (float)sizeCompartment.VolCurrent;
                circleOut.StrokeWidth = RadiusCalculator(currentVolume);
            }
            else
            {
                StrokeWidth = AverageFlow * Width;
                if (StrokeWidth > 30) StrokeWidth = 30;
                if (StrokeWidth < 2) StrokeWidth = 2;

                strokeStepsize = (StrokeWidth - currentStrokeWidth) / 10;
                currentStrokeWidth += strokeStepsize;
                if (Math.Abs(currentStrokeWidth - StrokeWidth) < Math.Abs(strokeStepsize))
                {
                    strokeStepsize = 0;
                    currentStrokeWidth = StrokeWidth;
                }

                circleOut.StrokeWidth = currentStrokeWidth;
            }

            SKRect mainRect = new SKRect(left, top, right, bottom);


            circleOut.Shader = SKShader.CreateLinearGradient(
                locationOrigen,
                locationTarget,
                new SKColor[] { colorFrom, colorTo },
                null,
                SKShaderTileMode.Mirror

            );

            offset.X = Math.Abs(locationOrigen.X - locationTarget.X) / 4f;

            using (SKPath path = new SKPath())
            {
                path.MoveTo(locationTarget);
                path.LineTo(locationOrigen);
                canvas.DrawPath(path, circleOut);
                canvas.DrawTextOnPath(Name, path, offset, textPaint2);
            }

            location1.X = locationOrigen.X - currentAngle * dx;
            location1.Y = locationOrigen.Y - currentAngle * dy;

            canvas.DrawCircle(location1.X + XOffset, location1.Y + YOffset, 7, paint);

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

        float RadiusCalculator(double vol)
        {
            float _radius, _cubicRadius;

            _cubicRadius = (float)(vol / ((4f / 3f) * Math.PI));
            _radius = (float)Math.Pow(_cubicRadius, 1f / 3f);

            return _radius * scale;

        }


    }
}
