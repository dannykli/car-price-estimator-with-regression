using System;
using System.Collections.Generic;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class encapsulating the methods for locating outliers
    internal class OutlierIdentfier
    {
        public DataTable data;
        public OutlierIdentfier(DataTable argData)
        {
            data = argData;
        }

        // Locates outliers using z-scores in a given column
        // params: column name, z-score threshold
        // returns: list of row numbers containing outliers
        public List<int> LocateOutliersWithZScores(string columnName, double threshold)
        {
            List<int> outlierRows = new List<int>();
            double[] columnValues = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            double mean = Statistics.CalculateMean(columnValues);
            double stdDev = Statistics.CalculateStdDev(columnValues);
            Console.WriteLine("Checking for outliers using Z-scores for column {0}...", columnName);
            for (int i = 0; i < columnValues.Length; i++)
            {
                double value = columnValues[i];
                double zScore = Statistics.CalculateZScore(value, mean, stdDev);
                if (Math.Abs(zScore) > threshold)
                {
                    outlierRows.Add(i);
                    //Console.WriteLine($"Outlier. Value: {value,-10} Z-Score: {zScore,-10} Row: {i,-5}");
                }
            }
            return outlierRows;
        }

        // Locates outliers using modified z-scores in a given column
        // params: column name, z-score threshold
        // returns: list of row numbers containing outliers
        public List<int> LocateOutliersWithModifiedZScores(string columnName, double threshold)
        {
            List<int> outlierRows = new List<int>();
            double[] columnValues = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            double median = Statistics.CalculateMedian(columnValues);
            double mad = Statistics.CalculateMedianAbsDev(columnValues);
            Console.WriteLine("Checking for outliers using modified Z-scores for column {0}...", columnName);
            for (int i = 0; i < columnValues.Length; i++)
            {
                double value = columnValues[i];
                double modZScore = Statistics.CalculateModifiedZScore(value, median, mad);
                if (Math.Abs(modZScore) > threshold)
                {
                    outlierRows.Add(i);
                    Console.WriteLine($"Outlier. Value: {value,-10} Modified Z-Score: {modZScore,-10} Row: {i,-5}");
                }
            }
            return outlierRows;
        }

        // Locates outliers using the IQR method in a given column
        // params: column name
        public void LocateOutliersWithIQR(string columnName)
        {
            double[] values = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            double[] sortedValues = new double[values.Length];
            Array.Copy(values, sortedValues, values.Length);
            Statistics.MergeSort(sortedValues);
            double q1 = Statistics.CalculatePercentile(sortedValues, 0.25);
            double q3 = Statistics.CalculatePercentile(sortedValues, 0.75);
            double iqr = q3 - q1;
            double lowerThreshold = q1 - 1.5 * iqr;
            double upperThreshold = q3 + 1.5 * iqr;
            Console.WriteLine("Checking for outliers using the IQR method for column {0}...", columnName);
            for (int i=0; i<data.Rows.Count; i++)
            {
                if (values[i] < lowerThreshold || values[i] > upperThreshold)
                {
                    Console.WriteLine($"Outlier. Value: {values[i],-10} Row: {i,-5}");
                }
            }
            Console.ReadLine();
        }
    }
}
