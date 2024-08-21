using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the methods for aiding feature selection
    internal class FeatureSelector
    {
        private DataTable data;

        public FeatureSelector(DataTable argData)
        {
            data = argData;
        }

        // Instantiates a scatter plot form to visualise the correlation of a continous predictor and the car price
        // params: column name
        public void DisplayScatterPlot(string columnName)
        {
            ScatterPlotForm form = new ScatterPlotForm(data, columnName, "Price");
        }

        // Instantiates a box plot form to visualise the distribution/skewness of non-binary predictors
        // params: array of column names
        public void DisplayBoxPlots(string[] columnNames)
        {
            BoxPlotForm form = new BoxPlotForm(data, columnNames);
        }

        // Outputs a ranking of how correlated the non-binary predictors are based on their pearson coefficient
        // params: array of column names
        public void RankNumericalColumnsByPearsonCoefficients(string[] columnNames)
        {
            Dictionary<string, double> columnCoeffPairs = new Dictionary<string, double>();
            double[] yValues = DataUtilities.GetColumnValuesAsDoubleArray(data, "Price");
            for (int i = 0; i < columnNames.Length; i++)
            {
                double[] xValues = DataUtilities.GetColumnValuesAsDoubleArray(data, columnNames[i]);
                double coeff = Statistics.CalculatePearsonCorrelationCoefficient(xValues, yValues);
                columnCoeffPairs.Add(columnNames[i], coeff);
            }
            var sortedList = columnCoeffPairs.OrderByDescending(kvp => Math.Abs(kvp.Value)).ToList(); // orders key-value pairs by value in descending order
            Console.WriteLine("Predictors ranked by their pearson correlation coefficient:");
            for (int i = 0; i < sortedList.Count; i++)
            {
                Console.WriteLine($"{i+1}. {sortedList[i].Key,-20} Coeff: {sortedList[i].Value}");
            }
        }

        // Outputs a ranking of how correlated the binary predictors are based on their point biserial correlation coefficient
        // params: array of column names
        public void RankBinaryColumnsByPointBiserialCoefficients(string[] columnNames)
        {
            Dictionary<string, double> columnCoeffPairs = new Dictionary<string, double>();
            double[] yValues = DataUtilities.GetColumnValuesAsDoubleArray(data, "Price");
            for (int i = 0; i < columnNames.Length; i++)
            {
                double[] xValues = DataUtilities.GetColumnValuesAsDoubleArray(data, columnNames[i]);
                double coeff = Statistics.CalculatePointBiserialCorrelationCoefficient(xValues, yValues);
                columnCoeffPairs.Add(columnNames[i], coeff);
            }
            var sortedList = columnCoeffPairs.OrderByDescending(kvp => Math.Abs(kvp.Value)).ToList(); // orders key-value pairs by value in descending order
            Console.WriteLine("Predictors ranked by their point biserial correlation coefficient:");
            for (int i = 0; i < sortedList.Count; i++)
            {
                Console.WriteLine($"{i+1}. {sortedList[i].Key,-20} Coeff: {sortedList[i].Value}");
            }
        }

        // Displays the percentage of 1s for each binary column passed
        // params: array of column names
        public void DisplayBinaryColumnRatios(string[] columnNames)
        {
            string s = "Binary Column Name:";
            Console.WriteLine($"{s,-20} Percentage of 1s:");
            for (int i = 0; i < columnNames.Length; i++)
            {
                double[] values = DataUtilities.GetColumnValuesAsDoubleArray(data, columnNames[i]);
                int n = values.Length;
                int noOfOnes = 0;
                foreach(var value in values)
                {
                    if (value == 1)
                    {
                        noOfOnes++;
                    }
                }
                double percentage = noOfOnes / (double)n * 100;
                percentage = Math.Round(percentage, 1);
                Console.WriteLine($"{columnNames[i],-20} {percentage}%");
            }
        }
    }
}
