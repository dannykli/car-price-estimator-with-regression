using System;
using System.Collections.Generic;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the methods for the data preprocessing stage
    internal class DataPreprocessor
    {
        public DataTable data;
        private OutlierIdentfier outlierIdentfier;
        private DataTableModifier dataTableModifier;

        // Constrcutor to instantiate the outlieridentfiier and datatablemodifier objects, and to populate the data attribute
        public DataPreprocessor(DataTable argData)
        {
            data = argData;
            outlierIdentfier = new OutlierIdentfier(data);
            dataTableModifier = new DataTableModifier(data);
        }

        // Delegates task of removing duplicate records to dataTableModifier instance
        public void RemoveDuplicateRecords()
        {
            dataTableModifier.RemoveDuplicateRecords();
        }

        // Checks if there are missing values and, if found, writes their indices to the console
        public void CheckForMissingValues()
        {
            bool hasMissingValues = false;
            foreach (DataRow row in data.Rows)
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(row.ItemArray[i].ToString()))
                    {
                        hasMissingValues = true;
                        Console.WriteLine("Missing value at row {0} column {1}", data.Rows.IndexOf(row), i);
                    }
                }
            }
            if (!hasMissingValues)
            {
                Console.WriteLine("No null/missing values.");
            }
        }

        // Delegates task of making the leather interior column numerical to dataTableModifier instance
        public void MakeLeatherInteriorColumnNumerical()
        {
            dataTableModifier.MakeLeatherInteriorColumnNumerical();
        }

        // Delegates task of splitting the engine volume column to dataTableModifier instance
        public void SplitEngineVolumeColumn()
        {
            dataTableModifier.SplitEngineVolumeColumn();
        }

        // Delegates task of making the mileage column numerical to dataTableModifier instance
        public void MakeMileageColumnNumerical()
        {
            dataTableModifier.MakeMileageColumnNumerical();
        }

        // Delegates task of making the wheel column numerical to dataTableModifier instance
        public void MakeWheelColumnNumerical()
        {
            dataTableModifier.MakeWheelColumnNumerical();
        }

        // Delegates task of making the category column numerical to dataTableModifier instance
        public void MakeCategoryColumnNumerical()
        {
            dataTableModifier.MakeCategoryColumnNumerical();
        }

        // Delegates task of making the fuel type column numerical to dataTableModifier instance
        public void MakeFuelTypeColumnNumerical()
        {
            dataTableModifier.MakeFuelTypeColumnNumerical();
        }

        // Delegates task of making the gearbox column numerical to dataTableModifier instance
        public void MakeGearBoxTypeColumnNumerical()
        {
            dataTableModifier.MakeGearBoxTypeColumnNumerical();
        }

        // Delegates task of making drive wheels column numerical to dataTableModifier instance
        public void MakeDriveWheelsColumnNumerical()
        {
            dataTableModifier.MakeDriveWheelsColumnNumerical();
        }

        // Delegates task of making colour column numerical to dataTableModifier instance
        public void MakeColourColumnNumerical()
        {
            dataTableModifier.MakeColourColumnNumerical();
        }

        // Delegates task of locating outliers using z-scores to outlierIdentifier instance
        // params: columnName, z-Score threshold
        // returns: list of row numbers containing outliers
        public List<int> LocateOutliersWithZScores(string columnName, double threshold)
        {
            return outlierIdentfier.LocateOutliersWithZScores(columnName, threshold);
        }

        // Delegates task of locating outliers using modified z-scores to outlierIdentifier instance
        // params: column name, mod. z-Score threshold
        // returns: list of row numbers containing outliers
        public List<int> LocateOutliersWithModifiedZScores(string columnName, double threshold)
        {
            return outlierIdentfier.LocateOutliersWithModifiedZScores(columnName, threshold);
        }

        // Delegates task of locating outliers using IQR to outlierIdentifier instance
        // params: column name
        public void LocateOutliersWithIQR(string columnName)
        {
            outlierIdentfier.LocateOutliersWithIQR(columnName);
        }

        // Imputes the mean value for a given data entry in a numerical (non-binary) column
        // params: column name, row number
        public void ImputeNumericalWithMean(string columnName, int rowNo)
        {
            double[] columnArray = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            data.Rows[rowNo][columnName] = Statistics.CalculateMean(columnArray);
        }

        // Imputes the mode binary value (1 or 0) for a given data entry in a binary column 
        // params: column name, row number
        public void ImputeBinaryWithMode(string columnName, int rowNo)
        {
            double[] columnArray = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            data.Rows[rowNo][columnName] = int.Parse(Statistics.CalculateMode(columnArray).ToString());
        }

        // Scales a given column's values using min-max scaling
        // params: column name
        public void ScaleNumericalColumnMinMax(string columnName)
        {
            DataColumn scaledColumn = new DataColumn(columnName + " scaled", typeof(double));
            data.Columns.Add(scaledColumn);
            double minValue = double.Parse(data.Rows[0][columnName].ToString());
            double maxValue = double.Parse(data.Rows[0][columnName].ToString());
            foreach(DataRow row in data.Rows)
            {
                double value = double.Parse(row[columnName].ToString());
                if (value < minValue)
                {
                    minValue = value;
                }
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }
            foreach(DataRow row in data.Rows)
            {
                double currentValue = double.Parse(row[columnName].ToString());
                double scaledValue = (currentValue - minValue) / (maxValue - minValue);
                row[columnName + " scaled"] = scaledValue;
            }
        }

        // Scales a given column's values using standardisation
        // params: column name
        public void ScaleNumericalColumnStandardisation(string columnName)
        {
            DataColumn scaledColumn = new DataColumn(columnName + " scaled", typeof(double));
            data.Columns.Add(scaledColumn);
            double[] columnArray = DataUtilities.GetColumnValuesAsDoubleArray(data, columnName);
            double mean = Statistics.CalculateMean(columnArray);
            double stdDev = Statistics.CalculateStdDev(columnArray);
            foreach (DataRow row in data.Rows)
            {
                double currentValue = double.Parse(row[columnName].ToString());
                double scaledValue = (currentValue - mean) / stdDev;
                row[columnName + " scaled"] = scaledValue;
            }
        }
    }
}
