using OxyPlot;

namespace RawViewer.Helpers;

public class PlotColorService 
{
    private static readonly OxyColor activeAnnotationColorDark = OxyColor.FromArgb(0xFF, 0xFF, 0xD6, 0x00);
    private static readonly OxyColor activeAnnotationColorLight = OxyColor.FromArgb(0xFF, 0x00, 0x00, 0x00);

    private static readonly OxyColor plotBackgroundColorDark = OxyColor.FromArgb(0xFF, 0x1a, 0x1a, 0x1a);
    private static readonly OxyColor plotBackgroundColorLight = OxyColor.FromArgb(0xFF, 0xD6, 0xD6, 0xD6);

    private static readonly OxyColor plotAreaBorderColorDark = OxyColor.FromArgb(0xFF, 0x59, 0x59, 0x59);
    private static readonly OxyColor plotAreaBorderColorLight = OxyColor.FromArgb(0xFF, 0xD6, 0xD6, 0xD6);

    private static readonly OxyColor legendTextColorDark = OxyColor.FromArgb(0xFF, 0xEF, 0xEF, 0xEF);
    private static readonly OxyColor legendTextColorLight = OxyColor.FromArgb(0xFF, 0x1E, 0x1E, 0x1E);

    private static readonly OxyColor axisTextColorDark = legendTextColorDark;
    private static readonly OxyColor axisTextColorLight = legendTextColorLight;

    private static readonly OxyColor majorGridLineColorDark = OxyColor.FromArgb(0x28, 0xFF, 0xFF, 0xFF);
    private static readonly OxyColor majorGridLineColorLight = OxyColor.FromArgb(0x28, 0x00, 0x00, 0x00);

    private static readonly OxyColor minorGridLineColorDark = OxyColor.FromArgb(0x14, 0xFF, 0xFF, 0xFF);
    private static readonly OxyColor minorGridLineColorLight = OxyColor.FromArgb(0x14, 0x00, 0x00, 0x00);

    public bool IsDarkTheme { get; set; }

    public PlotColorService()
    {
        IsDarkTheme = true;
    }

    public OxyColor ActiveAnnotationColor => IsDarkTheme ? activeAnnotationColorDark : activeAnnotationColorLight;
    public OxyColor PlotBackgroundColor => IsDarkTheme ? plotBackgroundColorDark : plotBackgroundColorLight;
    public OxyColor PlotAreaBorderColor => MajorGridLineColor;
    public OxyColor LegendTextColor => IsDarkTheme ? legendTextColorDark : legendTextColorLight;

    public OxyColor AxisTextColor => IsDarkTheme ? axisTextColorDark : axisTextColorLight;
    public OxyColor MajorGridLineColor => IsDarkTheme ? majorGridLineColorDark : majorGridLineColorLight;
    public OxyColor MinorGridLineColor => IsDarkTheme ? minorGridLineColorDark : minorGridLineColorLight;
}

