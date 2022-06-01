using OxyPlot;
using System;
using System.Linq;
using System.Windows.Media;

namespace JoeScan.LogScanner.Helpers;

public static class ColorDefinitions
{
    private static int maxNumLasers = 5;
    public static Color ColorForCableId(uint cableId, int laser = 0)
    {
        return colors[(cableId * maxNumLasers + laser) % colors.Length];
    }

    public static OxyColor OxyColorForCableId(uint cableId, int laser = 0)
    {
        Color tmp = ColorForCableId(cableId, laser);
        return OxyColor.FromArgb(tmp.A, tmp.R, tmp.G, tmp.B);
    }


    public static readonly Color[] LogColorValues =
        Enumerable.Range(0, 256).Select(s => HSL2RGB(0.1, 0.9, s / 255.0)).ToArray();




    private static readonly Color[] colors = {
            //red
            Color.FromArgb(255,255, 0, 0),
            Color.FromArgb(255,192, 0, 0),
            Color.FromArgb(255,128, 0, 0),
            Color.FromArgb(255,224, 0, 0),
            Color.FromArgb(255,160, 0, 0),

            //green
            Color.FromArgb(255,0, 255, 0),
            Color.FromArgb(255,0, 192, 0),
            Color.FromArgb(255,0, 128, 0),
            Color.FromArgb(255,0, 224, 0),
            Color.FromArgb(255,0, 160, 0),

            //magenta
            Color.FromArgb(255,255, 0, 255),
            Color.FromArgb(255,192, 0, 192),
            Color.FromArgb(255,128, 0, 128),
            Color.FromArgb(255,224, 0, 224),
            Color.FromArgb(255,160, 0, 160),

            //blue
            Color.FromArgb(255,0, 0, 255),
            Color.FromArgb(255,0, 0, 192),
            Color.FromArgb(255,0, 0, 128),
            Color.FromArgb(255,0, 0, 224),
            Color.FromArgb(255,0, 0, 160),

            //dark yellow - > brown
            Color.FromArgb(255,180,100,0),
            Color.FromArgb(255,140,130,0),
            Color.FromArgb(255,100,50,0),
            Color.FromArgb(255,140,100,0),
            Color.FromArgb(255,100,100,0),
		
           

            //cyan
            Color.FromArgb(255,0, 255, 255),
            Color.FromArgb(255,0, 192, 192),
            Color.FromArgb(255,0, 128, 128),
            Color.FromArgb(255,0, 224, 224),
            Color.FromArgb(255,0, 160, 160),

            //orange
            Color.FromArgb(255,255, 102, 0),
            Color.FromArgb(255,192,  62, 0),
            Color.FromArgb(255,128,  22, 0),
            Color.FromArgb(255,224,  82, 0),
            Color.FromArgb(255,160,  42, 0),

            //gray
            Color.FromArgb(255,160, 160, 160),
            Color.FromArgb(255,96, 96, 96),
            Color.FromArgb(255, 0,  0,  0),
            Color.FromArgb(255,128, 128, 128),
            Color.FromArgb(255, 64,  64,  64),
		
            // pink (only 4 elements ensures that more than 8 single laser scanners will wrap to new Colors)
            Color.FromArgb(255,255, 150, 255),
            Color.FromArgb(255,255, 110, 255),
            Color.FromArgb(255,255, 190, 255)
        };

    // Given H,S,L in range of 0-1
    // Returns a Color (RGB struct) in range of 0-255
    public static Color HSL2RGB(double h, double sl, double l)
    {
        double v;
        double r, g, b;

        r = l;   // default to gray
        g = l;
        b = l;
        v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
        if (v > 0)
        {
            double m;
            double sv;
            int sextant;
            double fract, vsf, mid1, mid2;

            m = l + l - v;
            sv = (v - m) / v;
            h *= 6.0;
            sextant = (int)h;
            fract = h - sextant;
            vsf = v * sv * fract;
            mid1 = m + vsf;
            mid2 = v - vsf;
            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;
                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;
                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;
                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;
                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }
        return Color.FromArgb(255, Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f), Convert.ToByte(b * 255.0f));
    }
}
