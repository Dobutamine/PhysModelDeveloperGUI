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
    public class AnimatedValve
    {

        public List<ValveConnector> connectors = new List<ValveConnector>();
        public BloodCompartment sizeCompartment;
        float dpi = 1;

        SKRect mainRect = new SKRect(0, 0, 0, 0);

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
        public string Title { get; set; } = "O2";
        public bool IsVisible { get; set; } = true;

        public float Width { get; set; } = 30;
        float StrokeWidth = 15;

        public float AverageFlow { get; set; } = 0;
        int averageCounter = 0;
        float tempAverageFlow = 0;

        float currentStrokeWidth = 0;
        float strokeStepsize = 0.1f;

        public AnimatedValve(float _dpi)
        {
            dpi = _dpi;

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
                Color = SKColors.White,
                IsStroke = false,
                TextSize = 14f / dpi


            };

            Width /= dpi * 1.5f;
        }

        public void AddConnector(ValveConnector c)
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


            float left = (float)Math.Sin(270 * 0.0174532925) * RadiusXOffset * radius;
            float right = (float)Math.Sin(90 * 0.0174532925) * RadiusXOffset * radius;
            float top = (float)Math.Cos(180 * 0.0174532925) * RadiusYOffset * radius;
            float bottom = (float)Math.Cos(0 * 0.0174532925) * RadiusYOffset * radius;



            // calculate the total volume and average spO2 if lumping is the case
            foreach (ValveConnector c in connectors)
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

            tempAverageFlow += totalFlow;

            if (averageCounter > 100)
            {
                AverageFlow = Math.Abs(tempAverageFlow) / averageCounter;
                tempAverageFlow = 0;
                averageCounter = 0;
            }       
            averageCounter++;

            //paint.Color = CalculateColor(totalSpO2 / connectors.Count);
            colorTo = AnimatedElementHelper.CalculateBloodColor(totalSpO2To / connectors.Count);
            colorFrom = AnimatedElementHelper.CalculateBloodColor(totalSpO2From / connectors.Count);

            currentAngle += totalFlow * Direction;
            if (Math.Abs(currentAngle) > Math.Abs(StartAngle - EndAngle))
            {
                currentAngle = 0;
            }

            if (sizeCompartment != null)
            {
                currentVolume = (float)sizeCompartment.VolCurrent;
                circleOut.StrokeWidth = AnimatedElementHelper.RadiusCalculator(currentVolume, scale);
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

            // calculate position
            locationOrigen = AnimatedElementHelper.GetPosition(StartAngle, radius, RadiusXOffset, RadiusYOffset);
            locationTarget = AnimatedElementHelper.GetPosition(EndAngle, radius, RadiusXOffset, RadiusYOffset);

            SKRect mainRect = new SKRect(left, top, right, bottom);


            circleOut.Shader = SKShader.CreateSweepGradient(
                new SKPoint(0f, 0f),
                new SKColor[] { colorFrom, colorTo },
                new float[] { StartAngle / 360f, EndAngle / 360f }
            );



            using (SKPath path = new SKPath())
            {
                path.AddArc(mainRect, StartAngle, Math.Abs(StartAngle - EndAngle));
                canvas.DrawPath(path, circleOut);
            }

            location1 = AnimatedElementHelper.GetPosition(StartAngle + currentAngle, radius, RadiusXOffset, RadiusYOffset);
            canvas.DrawCircle(location1.X + XOffset, location1.Y + YOffset, 7, paint);

        }




    }
}
