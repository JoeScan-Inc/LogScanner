using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Shared.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using System.Threading;
using System.Threading.Tasks;

namespace JoeScan.LogScanner.Desktop.TopAndSide
{


    public class TopAndSideViewModel : Screen
    {
       
        public PlotModel TopView { get; private set; }
        public PlotModel LeftView { get; private set; }

        public TopAndSideViewModel()
        {
            CreatePlotModels();
        }

        private void CreatePlotModels()
        {
            // TODO: create color helper to get style defined colors and create OxyColors
            // ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(
            //     new Uri("/F3H.LogScanner;component/Themes/Styles.xaml", UriKind.Relative));
            // var c = res["DarkBackgroundColor"] as Color? ?? default;
            TopView = new PlotModel()
            {
                PlotType = PlotType.Cartesian,
                Background = OxyColorsForStyle.PlotBackgroundColor,
                PlotAreaBorderColor = OxyColorsForStyle.PlotAreaBorderColor,
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(-5)
            };
            TopViewColumnAxis = new LinearAxis()
            {
                Minimum = 0,
                Maximum = 2560,
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
                MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
                TextColor = OxyColorsForStyle.AxisTextColor,
                IsZoomEnabled = true,
                LabelFormatter = x => null

            };
            TopView.Axes.Add(TopViewColumnAxis);

            TopViewRowAxis = new LinearAxis()
            {
                Position = AxisPosition.Right,

                Minimum = -400,
                Maximum = 400,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
                MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
                TextColor = OxyColorsForStyle.AxisTextColor,
                IsZoomEnabled = true,
                TickStyle = TickStyle.None,
                // LabelFormatter = x => null,

            };
            TopView.Axes.Add(TopViewRowAxis);
            LeftView = new PlotModel()
            {
                PlotType = PlotType.Cartesian,
                Background = OxyColorsForStyle.PlotBackgroundColor,
                PlotAreaBorderColor = OxyColorsForStyle.PlotAreaBorderColor,
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(-10)
            };
            LeftViewColumnAxis = new LinearAxis()
            {
                Minimum = 0,
                Maximum = 2560,
                Position = AxisPosition.Bottom,
                TickStyle = TickStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
                MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
                TextColor = OxyColorsForStyle.AxisTextColor,
                IsZoomEnabled = true
            };
            LeftView.Axes.Add(LeftViewColumnAxis);

            LeftViewRowAxis = new LinearAxis()
            {
                Position = AxisPosition.Right,
                Minimum = -400,
                Maximum = 400,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColorsForStyle.MajorGridLineColor,
                MinorGridlineColor = OxyColorsForStyle.MinorGridLineColor,
                TextColor = OxyColorsForStyle.AxisTextColor,
                IsZoomEnabled = true,
                TickStyle = TickStyle.None
                // LabelFormatter = x => null,
            };
            LeftView.Axes.Add(LeftViewRowAxis);
        }

        public LinearAxis LeftViewRowAxis { get; set; }

        public LinearAxis LeftViewColumnAxis { get; set; }

        public LinearAxis TopViewRowAxis { get; set; }

        public LinearAxis TopViewColumnAxis { get; set; }
       

       
    }
}
