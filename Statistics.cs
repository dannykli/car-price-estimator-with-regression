using System;
using System.Collections.Generic;

namespace RegressionAnalysisProj
{
    // Static class containing methods for a range of statistical calculations
    static internal class Statistics
    {
        // Calculates the mean of an array of values
        // params: array of values
        // returns: mean
        public static double CalculateMean(double[] values)
        {
            double sumOfValues = 0;
            foreach(double value in values)
            {
                sumOfValues += value;
            }
            double mean = sumOfValues / values.Length;
            return mean;
        }

        // Calculates the mode of an array of values
        // params: array of values
        // returns: mode
        public static double CalculateMode(double[] values)
        {
            Dictionary <double, int> valueCounter = new Dictionary<double, int>(); 
            foreach(double number in values)
            {
                if (valueCounter.ContainsKey(number))
                {
                    valueCounter[number]++;
                }

                else
                {
                    // adds a new key to dictionary and sets its value to 1
                    valueCounter[number] = 1;
                }
            }
            double mode = 0;
            int maxCount = 0;
            foreach (var pair in valueCounter)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    mode = pair.Key;
                }
            }
            return mode;
        }

        // Calculates the median of an array of values
        // If no. of values is odd, middle value is taken; if it is even, the mean from the two middle values is taken 
        // params: array of values
        // returns: median
        public static double CalculateMedian(double[] values)
        {
            double[] sortedValues = new double[values.Length];
            Array.Copy(values, sortedValues, values.Length);
            MergeSort(sortedValues);
            int i = 0;
            int n = sortedValues.Length;
            double median = 0;
            if (n % 2 == 1)
            {
                i = ((n + 1) / 2) - 1;
                median = sortedValues[i];
            }
            else
            {
                i = n / 2;
                double[] array = { sortedValues[i-1], sortedValues[i] };
                median = CalculateMean(array);
            }
            return median;
        }

        // Calculates the population variance of an array of values
        // params: values
        // returns: variance
        public static double CalculateVariance(double[] values)
        {
            double[] squaredDifferences = new double[values.Length];
            double mean = CalculateMean(values);
            for (int i = 0; i < squaredDifferences.Length; i++)
            {
                double squaredDifference = Math.Pow((values[i] - mean), 2);
                squaredDifferences[i] = squaredDifference;
            }
            double variance = CalculateMean(squaredDifferences);
            return variance;
        }

        // Calcualtes the standard deviation of an array of values
        // params: array of values
        // returns: standard deviation
        public static double CalculateStdDev(double[] values)
        {
            double stdDev = Math.Pow(CalculateVariance(values), 0.5);
            return stdDev;
        }

        // Calculates the covariance for two corresponding arrays of values (of a different variable)
        // params: array of values of variable x, array of values of variable y
        // returns: covariance
        public static double CalculateCoVariance(double[] xValues, double[] yValues)
        {
            double xMean = CalculateMean(xValues);
            double yMean = CalculateMean(yValues);
            double sumOfProducts = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                double xDifference = xValues[i] - xMean;
                double yDifference = yValues[i] - yMean;
                sumOfProducts += (xDifference * yDifference);
            }
            double covariance = sumOfProducts / (xValues.Length - 1);
            return covariance;
        }

        // Calculates the z-score for a value
        // params: value, mean, standard deviation
        // returns: z-score
        public static double CalculateZScore(double value, double mean, double stdDev)
        {
            double zScore = (value - mean) / stdDev;
            return zScore;
        }

        // Calculates the median absolute deviation of an array of values
        // params: array of values
        // returns: median absolute deviation
        public static double CalculateMedianAbsDev(double[] values)
        {
            double median = CalculateMedian(values);
            double[] absoluteDeviations = new double[values.Length];
            for (int i=0; i<values.Length; i++)
            {
                absoluteDeviations[i] = Math.Abs(values[i]-median);
            }
            double mad = CalculateMedian(absoluteDeviations);
            return mad;
        }

        // Calculates the modified z-score for a value
        // params: value, median, median absolute deviation
        // returns: modified z-score
        public static double CalculateModifiedZScore(double value, double median, double mad)
        {
            double modZScore = (0.6745 * (value - median)) / mad;
            return modZScore;
        }

        // Calculates a given percentile for an array of values
        // params: array of sorted values, percentile
        // returns: the value at that percentile
        public static double CalculatePercentile(double[] sortedValues, double percentile)
        {
            int n = sortedValues.Length;
            double index = (n - 1) * percentile;
            int lowerIndex = (int)Math.Floor(index); // rounds down to nearest integer
            int upperIndex = (int)Math.Ceiling(index); // rounds up to nearest integer

            if (lowerIndex == upperIndex)
            {
                return sortedValues[lowerIndex];
            }
            else
            {
                double lowerValue = sortedValues[lowerIndex];
                double upperValue = sortedValues[upperIndex];
                return lowerValue + (index - lowerIndex) * (upperValue - lowerValue);
            }
        }

        // Calculates the pearson correlation coefficient for an array of x values according to an array of y values
        // params: array of values of x variable, array of values of y variable
        // returns: pearson correlation coefficient
        public static double CalculatePearsonCorrelationCoefficient(double[] xValues, double[] yValues)
        {
            double xMean = CalculateMean(xValues);
            double yMean = CalculateMean(yValues);
            double xDifference, yDifference, productOfDifferences, sumOfProducts = 0, xSumOfSquaredDifferences = 0, ySumOfSquaredDifferences = 0;
            double pearsonCoeff;
            int n = xValues.Length;
            for (int i = 0; i < n; i++)
            {
                xDifference = xValues[i] - xMean;
                yDifference = yValues[i] - yMean;
                productOfDifferences = xDifference * yDifference;
                sumOfProducts += productOfDifferences;
                xSumOfSquaredDifferences += Math.Pow(xDifference, 2);
                ySumOfSquaredDifferences += Math.Pow(yDifference, 2);
            }
            pearsonCoeff = sumOfProducts / Math.Pow(xSumOfSquaredDifferences * ySumOfSquaredDifferences, 0.5);
            return pearsonCoeff;
        }

        // Calculates the point biserial correlation coefficient for an array of dichotomous x values according to an array of continous y values
        // params: array of values of dichotomous x variable, array of values of continous y variable
        // returns: point biserial correlation coefficient
        public static double CalculatePointBiserialCorrelationCoefficient(double[] xBinaryValues, double[] yValues)
        {
            List<double> yValuesWith0 = new List<double>();
            List<double> yValuesWith1 = new List<double>();
            for (int i=0; i<yValues.Length; i++)
            {
                if (xBinaryValues[i] == 0)
                {
                    yValuesWith0.Add(yValues[i]);
                }
                else if (xBinaryValues[i] == 1)
                {
                    yValuesWith1.Add(yValues[i]);
                }
            }
            double meanY0 = CalculateMean(yValuesWith0.ToArray());
            double meanY1 = CalculateMean(yValuesWith1.ToArray());
            double stdDev = CalculateStdDev(yValues);
            int n0 = yValuesWith0.Count;
            int n1 = yValuesWith1.Count;
            int n = yValues.Length;
            double correctionFactor = Math.Sqrt(n0 * n1 / (n * (n - 1.0)));
            double coeff = (meanY1 - meanY0) * correctionFactor / stdDev;
            return coeff;
        }

        // Sorts an array into ascending order using the merge sort algorithm
        // params: array of values of the double datatype
        public static void MergeSort(double[] array)
        {
            if (array.Length <= 1)
            {
                return;
            }
            int mid = array.Length / 2;
            double[] leftArr = new double[mid];
            double[] rightArr = new double[array.Length - mid];

            Array.Copy(array, leftArr, mid);
            Array.Copy(array, mid, rightArr, 0, array.Length - mid);

            // Recursive calls to sort the two sub-arrays
            MergeSort(leftArr);
            MergeSort(rightArr);

            // Merge the two sorted sub-arrays
            Merge(array, leftArr, rightArr);
        }

        // Merges two sub-arrays into the original array
        // params: original array, left sub-array, right sub-array
        private static void Merge(double[] array, double[] leftArr, double[] rightArr)
        {
            int i = 0, j = 0, k = 0;

            // Compares elements from two arrays and stops merging once one of the entire arrays have been merged
            while (i < leftArr.Length && j < rightArr.Length)
            {
                if (leftArr[i] <= rightArr[j])
                {
                    array[k] = leftArr[i];
                    i++;
                }
                else
                {
                    array[k] = rightArr[j];
                    j++;
                }
                k++;
            }

            // Remaining elements copied (if any)
            while (i < leftArr.Length)
            {
                array[k] = leftArr[i];
                i++;
                k++;
            }
            while (j < rightArr.Length)
            {
                array[k] = rightArr[j];
                j++;
                k++;
            }
        }
    }
}
