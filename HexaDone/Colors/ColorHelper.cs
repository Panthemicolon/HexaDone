using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Annotations;
using System.Windows.Media;

namespace HexaDone
{
    public static class ColorHelper
    {
        private static Random random = new Random();
        private static byte mutedMin = 0x10;
        private static byte mutedMax = 0x30;

        public static HSVColor GetRandomHSVColor(bool muted = false)
        {
            HSVColor color = new HSVColor(random.Next(361), muted ? (float)random.Next(mutedMin, mutedMax) / 255.0f : 1.0f, 0.5f + ((float)random.NextDouble() * 0.5f));
            return color;
        }


        public static HSVColor GetMutedVariant(HSVColor color, float saturation = 0.05f, float minValue = 0.95f)
        {
            float s = Math.Min(saturation > 1 ? saturation / 255.0f : saturation, color.S);
            return new HSVColor(color.H, s, Math.Max(color.V, minValue));
        }

        public static Color GetRandomColor(bool muted = false)
        {
            byte max = 0xA0;
            byte min = 0x00;
            byte r = (byte)(0xFF - random.Next(min, max));
            byte g = (byte)(0xFF - random.Next(min, max));
            byte b = (byte)(0xFF - random.Next(min, max));
            Color color = Color.FromRgb(r, g, b);
            if (muted)
                color = GetMutedVariant(color);
            return color;
        }


        public static Color GetMutedVariant(Color color)
        {
            Color col = color;
            byte c = (byte)(0xFF - random.Next(mutedMin, mutedMax));

            if (col.R == col.G && col.R == col.B) // if we have a gray Value, just return a random muted Gray Variaton
            {
                return Color.FromRgb(c, c, c);
            }
            if (col.R > col.G && col.R > col.B)
            {
                int delta = c - Math.Max(col.G, col.B);
                return Color.FromRgb(col.R, (byte)(col.G + delta), (byte)(col.B + delta));
            }
            else if (col.G > col.R && col.G > col.B)
            {
                int delta = c - Math.Max(col.R, col.B);
                return Color.FromRgb((byte)(col.R + delta), col.G, (byte)(col.B + delta));
            }
            else
            {
                int delta = c - Math.Max(col.R, col.G);
                return Color.FromRgb((byte)(col.R + delta), (byte)(col.G + delta), col.B);
            }
        }

        public static Color GetGrayScale(Color color)
        {
            Color col = color;
            float c = 0.2126f * col.R + 0.7152f * col.G + 0.0722f * col.B;
            return Color.FromRgb((byte)c, (byte)c, (byte)c);
        }
    }
}
