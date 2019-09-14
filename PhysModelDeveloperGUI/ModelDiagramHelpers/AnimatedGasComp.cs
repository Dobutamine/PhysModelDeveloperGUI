using PhysModelLibrary.Compartments;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysModelDeveloperGUI
{
    public class AnimatedGasComp
    {
        // list of all compartmentn contained in this animated compartment
        public List<GasCompartment> compartments = new List<GasCompartment>();


        SKPaint paint;

        SKPaint textPaint;

        public SKPoint location = new SKPoint(0, 0);

        public float scaleRelative = 50;
        float scale = 1;
        public float Degrees { get; set; } = 0;
        public float RadiusXOffset { get; set; } = 1;
        public float RadiusYOffset { get; set; } = 1;
        public string Name { get; set; } = "X";
        public bool IsVisible { get; set; } = true;
        float dpi = 1f;

        public AnimatedGasComp(float _dpi)
        {
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
                TextSize = 14f / dpi
            };

            

        }
        public void AddCompartment(GasCompartment c)
        {
            compartments.Add(c);
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

            float remap = Remap((float)sat, 0, 15, 0, 1);

            if (remap < 0) remap = 0;

            byte red = (byte)(remap * 250);
            byte green = (byte)(remap * 100);
            byte blue = (byte)(80 + remap * 75);
            SKColor sKColor = new SKColor(red, green, blue);
            return sKColor;
        }

        public void DrawCompartment(SKCanvas canvas, float _radX, float _radY)
        {
            float totalVolume = 0;
            float totalSpO2 = 0;
            float radius = 0;


            scale = _radX * scaleRelative / dpi;
            radius = _radX / 2.5f;

            if (_radX > _radY)
            {
                scale = _radY * scaleRelative / dpi;
                radius = _radY / 2.5f;
            }

            // calculate position
            location = GetPosition(Degrees, radius);

            // calculate the total volume and average spO2 if lumping is the case
            foreach (GasCompartment c in compartments)
            {
                totalVolume += (float)c.VolCurrent;
                totalSpO2 += (float)c.PO2;
            }

            float twidth = textPaint.MeasureText(Name);

            paint.Color = CalculateColor(totalSpO2 / compartments.Count);

            float r = RadiusCalculator(totalVolume);


            canvas.DrawCircle(location, r, paint);
            canvas.DrawText(Name, location.X - twidth / 2, location.Y + 7, textPaint);
        }

        float RadiusCalculator(double vol)
        {
            float _radius, _cubicRadius;

            _cubicRadius = (float)(vol / ((4f / 3f) * Math.PI));
            _radius = (float)Math.Pow(_cubicRadius, 1f / 3f);

            return _radius * scale;

        }

        SKPoint GetPosition(float degrees, float _rad)
        {
            SKPoint point = new SKPoint
            {
                X = (float)Math.Cos(degrees * 0.0174532925) * RadiusXOffset * _rad,
                Y = (float)Math.Sin(degrees * 0.0174532925) * RadiusYOffset * _rad
            };

            return point;
        }
    }
}
