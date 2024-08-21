using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot;
using System;
using System.Data;
using System.Windows.Forms;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the implementation to display a form with boxplots
    internal class BoxPlotForm
    {
        private DataTable data;
        private string[] variables;
        public BoxPlotForm(DataTable argData, string[] variables)
        {
            Application.EnableVisualStyles();

            data = argData;
            this.variables = variables;

            PlotModel plotModel = CreateBoxPlot();

            // Displays the box plot in a Windows Form
            var plotForm = new Form();
            var plotView = new PlotView();
            plotView.Dock = DockStyle.Fill;
            plotView.Model = plotModel;
            plotForm.Controls.Add(plotView);


            // Shows the form
            Application.Run(plotForm);
        }

        // Creates a plotmodel object with a boxplot series, consisting of 1 or more individual box plots
        private PlotModel CreateBoxPlot()
        {
            var plotModel = new PlotModel { Title = "Box Plot" };

            var boxPlotSeries = new BoxPlotSeries()
            {
                Fill = OxyColors.LightGray
            };
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
            };

            int i;
            for (i = 0; i < variables.Length; i++)
            {
                double[] array = DataUtilities.GetColumnValuesAsDoubleArray(data, variables[i]);
                double[] sortedArray = new double[array.Length];
                Array.Copy(array, sortedArray, array.Length);
                Statistics.MergeSort(sortedArray);
                double min = Statistics.CalculatePercentile(sortedArray, 0);
                double q1 = Statistics.CalculatePercentile(sortedArray, 0.25);
                double q2 = Statistics.CalculatePercentile(sortedArray, 0.5);
                double q3 = Statistics.CalculatePercentile(sortedArray, 0.75);
                double max = Statistics.CalculatePercentile(sortedArray, 1);
                BoxPlotItem bp = new BoxPlotItem(i, min, q1, q2, q3, max);
                boxPlotSeries.Items.Add(bp);
                xAxis.Labels.Add(variables[i]);
            }
            BoxPlotItem emptyBp = new BoxPlotItem(i, 0, 0, 0, 0, 0);
            boxPlotSeries.Items.Add(emptyBp);

            plotModel.Series.Add(boxPlotSeries);

            plotModel.Axes.Add(xAxis);
            return plotModel;
        }
    }
}
