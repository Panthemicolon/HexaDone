using System;
using System.Collections.Generic;
using System.Text;

namespace HexaDone
{
    public struct HSVColor
    {
        public float H { get; set; }

        public float S { get; set; }

        public float V { get; set; }

        public HSVColor(float h, float s, float v)
        {
            this.H = h % 360.0f;
            this.S = s > 1 ? s / 255.0f : s;
            this.V = v > 1 ? v / 255.0f : v;
        }

        public (byte r, byte g, byte b) ToRGB()
        {
            double chroma = this.V * this.S;
            double hue = this.H / 60.0;
            double x = chroma * (1 - Math.Abs(hue % 2 - 1));
            double delta = this.V - chroma;
            switch ((int)hue)
            {
                case 0:
                    return ((byte)((chroma + delta) * 255), (byte)((x + delta) * 255), (byte)(delta * 255));
                case 1:
                    return ((byte)((x + delta) * 255), (byte)((chroma + delta) * 255), (byte)(delta * 255));
                case 2:
                    return ((byte)(delta * 255), (byte)((chroma + delta) * 255), (byte)((x + delta) * 255));
                case 3:
                    return ((byte)((delta) * 255), (byte)((x + delta) * 255), (byte)((chroma + delta) * 255));
                case 4:
                    return ((byte)((x + delta) * 255), (byte)(delta * 255), (byte)((chroma + delta) * 255));
                case 5:
                    return ((byte)((chroma + delta) * 255), (byte)(delta * 255), (byte)((x + delta) * 255));
                default:
                    return ((byte)(delta * 255), (byte)(delta * 255), (byte)(delta * 255));
            }
        }

        public string ToRgbHexString()
        {
            (byte r, byte g, byte b) rgb = this.ToRGB();
            return $"#{rgb.r.ToString("X2")}{rgb.g.ToString("X2")}{rgb.b.ToString("X2")}";
        }

        public static HSVColor FromRGB(byte r, byte g, byte b)
        {
            HSVColor color = new HSVColor();
            float r1 = r / 255.0f;
            float g1 = g / 255.0f;
            float b1 = b / 255.0f;

            float max = MathF.Max(MathF.Max(r1, g1), b1);
            float min = MathF.Min(MathF.Min(r1, g1), b1);
            float delta = max - min;


            color.V = max;
            color.S = max == 0 ? 0 : delta / max;

            if (delta == 0.0f)
            {
                color.H = 0;
            }
            else if (max == r1)
            {
                color.H = 60.0f * ((g1 - b1) / delta);
            }
            else if (max == g1)
            {
                color.H = 60.0f * (2 + ((b1 - r1) / delta));
            }
            else // max == b1
            {
                color.H = 60.0f * (4 + ((r1 - g1) / delta));
            }

            return color;
        }

        public static HSVColor FromRgbHexString(string hex)
        {
            string hexrgb = hex;
            if (hexrgb.StartsWith('#'))
                hexrgb = hexrgb.Substring(1);

            if(hexrgb.Length == 8)
            {
                hexrgb = hexrgb.Substring(2);
            }

            if(hexrgb.Length == 6)
            {
                return HSVColor.FromRGB(Convert.ToByte(hexrgb.Substring(0, 2), 16), Convert.ToByte(hexrgb.Substring(2, 2), 16), Convert.ToByte(hexrgb.Substring(4, 2), 16));
            }
            else
            {
                return new HSVColor(0, 0, 0);
            }

            
        }
    }
}
