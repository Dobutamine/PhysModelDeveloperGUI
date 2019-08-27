using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysModelDeveloperGUI
{
    public static class AnimatedElementHelper
    {
        public static SKPoint GetPosition(float _degrees, float _rad, float _radXOffset, float _radYOffset)
        {
            SKPoint point = new SKPoint
            {
                X = (float)Math.Cos(_degrees * 0.0174532925) * _radXOffset * _rad,
                Y = (float)Math.Sin(_degrees * 0.0174532925) * _radYOffset * _rad
            };

            return point;
        }

        public static float RadiusCalculator(double vol, float scale)
        {
            float _radius, _cubicRadius;

            _cubicRadius = (float)(vol / ((4f / 3f) * Math.PI));
            _radius = (float)Math.Pow(_cubicRadius, 1f / 3f);

            return _radius * scale;

        }

        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static SKColor CalculateBloodColor(double sat)
        {
            // sat can variate between 0 - 10
            if (sat > 10)
            {
                sat = 10;
            }

            float remap = AnimatedElementHelper.Remap((float)sat, 0, 10, -2f, 1);

            if (remap < 0) remap = 0;

            byte red = (byte)(remap * 200);
            byte green = (byte)(remap * 75);
            byte blue = (byte)(80 + remap * 75);

            return new SKColor(red, green, blue);
        }
    }
}
