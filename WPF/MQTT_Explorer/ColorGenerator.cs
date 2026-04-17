using System;
using System.Collections.Generic;
using System.Windows.Media;


//chatGPT
public static class ColorGenerator
{
    public static List<SolidColorBrush> GenerateDistinctColors(int count)
    {
        List<SolidColorBrush> colors = new List<SolidColorBrush>();

        for (int i = 0; i < count; i++)
        {
            double hue = i * (360.0 / count);
            Color color = HsvToRgb(hue, 1, 1);
            colors.Add(new SolidColorBrush(color));
        }

        return colors;
    }

    private static Color HsvToRgb(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r, g, b;

        if (h < 60)
        {
            r = c; g = x; b = 0;
        }
        else if (h < 120)
        {
            r = x; g = c; b = 0;
        }
        else if (h < 180)
        {
            r = 0; g = c; b = x;
        }
        else if (h < 240)
        {
            r = 0; g = x; b = c;
        }
        else if (h < 300)
        {
            r = x; g = 0; b = c;
        }
        else
        {
            r = c; g = 0; b = x;
        }

        byte red = (byte)((r + m) * 255);
        byte green = (byte)((g + m) * 255);
        byte blue = (byte)((b + m) * 255);

        return Color.FromRgb(red, green, blue);
    }

    public static SolidColorBrush GetMostDifferentColor(List<SolidColorBrush> existingColors)
    {
        // Convertir les couleurs existantes en teintes (0-360)
        var existingHues = existingColors.Select(c => RgbToHsv(c.Color).Item1).ToList();

        // Trouver le plus grand écart entre les teintes
        double largestGap = 0;
        double gapStartHue = 0;

        existingHues.Sort();
        for (int i = 0; i < existingHues.Count; i++)
        {
            double currentHue = existingHues[i];
            double nextHue = existingHues[(i + 1) % existingHues.Count];

            double gap = (nextHue - currentHue + 360) % 360;
            if (gap > largestGap)
            {
                largestGap = gap;
                gapStartHue = currentHue;
            }
        }

        // Calculer la nouvelle teinte au milieu du plus grand écart
        double newHue = (gapStartHue + largestGap / 2) % 360;

        // Convertir la nouvelle teinte en couleur RGB
        return new SolidColorBrush(HsvToRgb(newHue, 1, 1));
    }

    private static (double, double, double) RgbToHsv(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double hue = 0;
        if (delta != 0)
        {
            if (max == r)
                hue = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                hue = 60 * (((b - r) / delta) + 2);
            else
                hue = 60 * (((r - g) / delta) + 4);
        }

        hue = (hue + 360) % 360;
        double saturation = (max == 0) ? 0 : delta / max;
        double value = max;

        return (hue, saturation, value);
    }

}
