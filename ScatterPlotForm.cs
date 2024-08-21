using System;
using System.Data;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace RegressionAnalysisProj
{
    // Class that contains the logic for creating a form with a scatter plot
    internal class ScatterPlotForm
    {
        private DataTable data;
        private string xColumnName;
        private string yColumnName;
        public ScatterPlotForm(DataTable data, string xColumnName, string yColumnName)
        {
            Application.EnableVisualStyles();

            this.data = data;
            this.xColumnName = xColumnName;
            this.yColumnName = yColumnName;

            PlotModel plotModel = CreateScatterPlot();

            // Displays the plot in a Windows Form
            var plotForm = new Form();
            var plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            plotView.Model = plotModel;
            plotForm.Controls.Add(plotView);

            // Shows the form
            Application.Run(plotForm);
        }

        // Creates the scatter plot
        // returns: plot model
        private PlotModel CreateScatterPlot()
        {
            var plotModel = new PlotModel { Title = "Scatter Plot" };
            var scatterSeries = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3
            };

            foreach (DataRow row in data.Rows)
            {
                double x = Convert.ToDouble(row[xColumnName]);
                double y = Convert.ToDouble(row[yColumnName]);
                scatterSeries.Points.Add(new ScatterPoint(x, y));
            }

            var xAxis = new LinearAxis { Position = AxisPosition.Bottom, Title = xColumnName };
            var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = yColumnName };
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            plotModel.Series.Add(scatterSeries);
            return plotModel;
        }
    }
}
