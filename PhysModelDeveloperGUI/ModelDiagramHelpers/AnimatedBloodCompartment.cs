using PhysModelLibrary.Compartments;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysModelDeveloperGUI
{
    public class AnimatedBloodCompartment
    {
        // list of all compartmentn contained in this animated compartment
        public List<BloodCompartment> compartments = new List<BloodCompartment>();

        double pumpLocation = 0;

        SKPaint circleOut = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true,
            Color = SKColors.Orange,
            StrokeWidth = 5,
        };

        SKPaint pumpColorStroke = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = false,
            Color = SKColors.White,
            StrokeWidth = 5,
        };


        SKPaint paint = new SKPaint()
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = false,
            Color = SKColors.Blue,
            StrokeWidth = 10
        };

        SKPaint textPaint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Arial Bold"),
            Style = SKPaintStyle.Fill,
            FakeBoldText = true,
            IsAntialias = true,
            Color = SKColors.White,
            IsStroke = false,
            TextSize = 20f
        };
        SKPaint textPaint2 = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Arial Bold"),
            Style = SKPaintStyle.Fill,
            FakeBoldText = true,
            IsAntialias = true,
            Color = SKColors.White,
            IsStroke = false,
            TextSize = 18f
        };
        public float OffsetXFactor = 2.5f;
        public SKPoint offset = new SKPoint
        {
            X = 8,
            Y = 8

        };
        SKRect mainRect = new SKRect(0, 0, 0, 0);

        public SKPoint location = new SKPoint(0, 0);
        public SKPoint locationOrigen = new SKPoint(0, 0);
        public SKPoint locationTarget = new SKPoint(0, 0);
        public SKPoint location1 = new SKPoint(0, 0);
        public SKPoint location2 = new SKPoint(0, 0);
        public SKPoint location3 = new SKPoint(0, 0);
        public SKPoint location4 = new SKPoint(0, 0);

        public float scaleRelative = 50;
        float scale = 1;
        public float Degrees { get; set; } = 0;
        public float StartAngle { get; set; } = 0;
        public float EndAngle { get; set; } = 0;
        public float Direction { get; set; } = 1;
        public float Speed { get; set; } = 0.05f;
        public bool IsVisible { get; set; } = true;

        public bool IsVessel { get; set; } = false;
        public float RadiusXOffset { get; set; } = 1;
        public float RadiusYOffset { get; set; } = 1;
        public string Name { get; set; } = "X";

        float dpi = 1;

        public AnimatedBloodCompartment(float _dpi)
        {
            dpi = _dpi;

            circleOut = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true,
                Color = SKColors.Orange,
                StrokeWidth = 5,
            };

            pumpColorStroke = new SKPaint()
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = false,
                Color = SKColors.White,
                StrokeWidth = 5,
            };


            paint = new SKPaint()
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = false,
                Color = SKColors.Blue,
                StrokeWidth = 10
            };

            textPaint = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Arial Bold"),
                Style = SKPaintStyle.Fill,
                FakeBoldText = true,
                IsAntialias = true,
                Color = SKColors.White,
                IsStroke = false,
                TextSize = 20f / dpi
            };
            textPaint2 = new SKPaint
            {
                Typeface = SKTypeface.FromFamilyName("Arial Bold"),
                Style = SKPaintStyle.Fill,
                FakeBoldText = true,
                IsAntialias = true,
                Color = SKColors.White,
                IsStroke = false,
                TextSize = 18f / dpi
            };

            
        }
        public void AddCompartment(BloodCompartment c)
        {
            compartments.Add(c);
        }

        public void DrawCompartment(SKCanvas canvas, float _radX, float _radY)
        {
            float totalVolume = 0;
            float totalSpO2 = 0;
            float radius = 0;

            
            scale = _radX * scaleRelative * dpi;
            radius = _radX / 2.5f;

            if (_radX > _radY)
            {
                scale = _radY * scaleRelative * dpi;
                radius = _radY / 2.5f;
            }


            // calculate the total volume and average spO2 if lumping is the case
            foreach (BloodCompartment c in compartments)
            {
                totalVolume += (float)c.VolCurrent;
                totalSpO2 += (float)c.TO2;
            }

            paint.Color = AnimatedElementHelper.CalculateBloodColor(totalSpO2 / compartments.Count);

            float twidth = textPaint.MeasureText(Name);

            // calculate position
            location = AnimatedElementHelper.GetPosition(Degrees, radius, RadiusXOffset, RadiusYOffset);

            // calculate position
            locationOrigen = AnimatedElementHelper.GetPosition(StartAngle, radius, RadiusXOffset, RadiusYOffset);
            locationTarget = AnimatedElementHelper.GetPosition(EndAngle, radius, RadiusXOffset, RadiusYOffset);

            float left = (float)Math.Sin(270 * 0.0174532925) * RadiusXOffset * radius;
            float right = (float)Math.Sin(90 * 0.0174532925) * RadiusXOffset * radius;
            float top = (float)Math.Cos(180 * 0.0174532925) * RadiusYOffset * radius;
            float bottom = (float)Math.Cos(0 * 0.0174532925) * RadiusYOffset * radius;

            float r = AnimatedElementHelper.RadiusCalculator(totalVolume, scale);

            if (IsVessel)
            {
                mainRect.Left = left;
                mainRect.Top = top;
                mainRect.Right = right;
                mainRect.Bottom = bottom;

                using (SKPath path = new SKPath())
                {
                    path.AddArc(mainRect, StartAngle, Math.Abs(StartAngle - EndAngle) * Direction);
                    circleOut.Color = paint.Color;

                    circleOut.StrokeWidth = r;
                    canvas.DrawPath(path, circleOut);

                    offset.X = Math.Abs(locationOrigen.X - locationTarget.X) / OffsetXFactor;
                    canvas.DrawTextOnPath(Name, path, offset, textPaint2);

                }
            }
            else
            {
                paint.StrokeWidth = 10;
                canvas.DrawCircle(location, r, paint);
                canvas.DrawText(Name, location.X - twidth / 2, location.Y + 7, textPaint);

                if (compartments[0].IsPump)
                {
                    SKPoint or = new SKPoint(0, 0);
                    SKPoint dest = new SKPoint(0, 0);
                    or = location;
                    dest = location;
                    or.X = location.X + (float)Math.Cos(pumpLocation) * 40;
                    or.Y = location.Y + (float)Math.Sin(pumpLocation) * 40;
                    dest.X = location.X + (float)Math.Cos(pumpLocation + Math.PI) * 40;
                    dest.Y = location.Y + (float)Math.Sin(pumpLocation + Math.PI) * 40;

                    pumpLocation += compartments[0].dataCollector.VolumeFlowOut / 10000;
                    if (pumpLocation > 2 * Math.PI)
                    {
                        pumpLocation = 0;
                    }

                    canvas.DrawLine(or, dest, pumpColorStroke);
                }


            }



        }


    }
}
