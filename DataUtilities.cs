using System;
using System.Data;
using System.Linq;

namespace RegressionAnalysisProj
{
    // Static class containing useful fuctionality regarding data
    static internal class DataUtilities
    {
        // Returns an array of values (of double datatype) in a given column
        // params: data, column name
        // returns: array of columns values
        public static double[] GetColumnValuesAsDoubleArray(DataTable data, string columnName)
        {
            double[] array = new double[data.Rows.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Convert.ToDouble(data.Rows[i][columnName]);
            }
            return array;
        }

        // Displays a given number of rows of data
        // params: data, number of rows
        public static void DisplayData(DataTable data, int noOfRows)
        {
            int rowNo = 0;
            foreach (DataRow row in data.Rows)
            {
                Console.WriteLine();
                foreach (DataColumn column in data.Columns)
                {
                    Console.Write($"{row[column],-20}");
                    if (row[column] == null)
                    {
                        Console.WriteLine("***null value***");
                    }
                }
                Console.WriteLine();
                rowNo++;
                if (rowNo == noOfRows)
                {
                    break;
                }
            }
        }

        // Displays given rows
        // params: data, row numbers
        public static void DisplayRows(DataTable data, int[] rowNumbers)
        {
            Console.WriteLine();
            foreach (int rowNo in rowNumbers)
            {
                foreach (DataColumn column in data.Columns)
                {
                    Console.Write($"{data.Rows[rowNo][column],-20}");
                    if (data.Rows[rowNo][column] == null)
                    {
                        Console.WriteLine("***null value***");
                    }
                }
                Console.WriteLine();
            }
            
        }

        // Displays all column names of the data
        // params: data
        public static void DisplayColumnNames(DataTable data)
        {
            foreach (DataColumn column in data.Columns)
            {
                Console.Write($"{column.ColumnName,-20}");
            }
            Console.WriteLine();
        }

        // Splits data into training, validation and holdout data.
        // params: data, trainingRatio, validationRatio
        // returns: array containing training, validationa and holdout datatables
        public static DataTable[] SplitData(DataTable data, double trainingRatio, double validationRatio)
        {
            int rowCount = data.Rows.Count;
            int trainingRowCount = (int)Math.Round(rowCount * trainingRatio);
            int validationRowCount = (int)Math.Round(rowCount * validationRatio);
            Random rnd = new Random(1); // 1 is for fixed random seed so the data division does not affect regression model performance
            var shuffledRows = data.AsEnumerable().OrderBy(row => rnd.Next()).ToArray(); // shuffled the rows randomly into an array
            DataTable trainingData = data.Clone();
            DataTable validationData = data.Clone();
            DataTable holdoutData = data.Clone();
            for (int i = 0; i < rowCount; i++)
            {
                if (i < trainingRowCount)
                {
                    trainingData.ImportRow(shuffledRows[i]);
                }
                else if (i < trainingRowCount + validationRowCount)
                {
                    validationData.ImportRow(shuffledRows[i]);
                }
                else
                {
                    holdoutData.ImportRow(shuffledRows[i]);
                }
            }
            DataTable[] dataArray = { trainingData, validationData, holdoutData };
            return dataArray;
        }


        // Divides data into 4 equal subsets
        // params: data
        // returns: array of divided datatables
        public static DataTable[] DivideDataInto4(DataTable data)
        {
            int rowCount = data.Rows.Count;
            Random rnd = new Random(1); // 1 is for fixed random seed so the data division does not affect regression model performance
            var shuffledRows = data.AsEnumerable().OrderBy(row => rnd.Next()).ToArray(); // shuffled the rows randomly into an array
            int splitSize = rowCount / 4;
            int remainder = rowCount % 4;
            int startIndex = 0;
            DataTable[] dataArray = new DataTable[4];
            for (int i = 0; i < 4; i++)
            {
                int endIndex = startIndex + splitSize;
                if (i < remainder)
                {
                    endIndex++;
                }
                DataTable dataSubset = data.Clone();
                for (int j = startIndex; j < endIndex; j++)
                {
                    dataSubset.ImportRow(shuffledRows[j]);
                }
                dataArray[i] = dataSubset;
                startIndex = endIndex;
            }
            return dataArray;
        }
    }
}
