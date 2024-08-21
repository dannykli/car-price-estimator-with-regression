using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the initial validation of the dataset
    public class DataValidator
    {
        private DataTable data;
        private DataTable dataDict;
        private bool fileReadFailed;

        public DataValidator(string dataFilename, string dataDictFilename)
        {
            ReadDataDictionaryFile(dataDictFilename);
            ReadDataFile(dataFilename);
        }

        public DataTable GetData()
        {
            return data;
        }

        // Validates data using the data dictionary
        public void ValidateData()
        {
            if (!fileReadFailed)
            {
                Console.WriteLine();
                Console.WriteLine("Checking data validity...");
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    string regExPattern = dataDict.Rows[i].Field<string>("Regular expression pattern");
                    foreach (DataRow row in data.Rows)
                    {
                        bool valid = ValidateDataEntry(row[i].ToString(), regExPattern);
                        if (!valid)
                        {
                            // Some special characters in the data have a character encoding issue so are displayed as '?'
                            // ?s are replaced with _ so it will be known in the later stages of regression analysis that this data has some lost characters 
                            Console.WriteLine(String.Format("Invalid: {0,-30} {1,-5} {2,-5}", row[i], data.Rows.IndexOf(row).ToString(), i.ToString()));
                            Char[] temp = row[i].ToString().ToCharArray();
                            for (int j = 0; j < temp.Length; j++)
                            {
                                if (char.ConvertToUtf32(row[i].ToString(), j) > 255)
                                {
                                    temp[j] = '_';
                                }
                            }
                            string updatedValue = new string(temp);
                            row[i] = updatedValue;
                            Console.WriteLine("Updated: " + row[i]);
                            Console.WriteLine();
                        }
                    }
                }

                // Final check after updated data entries
                Console.WriteLine("Confirming data validity...");
                int invalidCount = 0;
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    string regExPattern = dataDict.Rows[i].Field<string>("Regular expression pattern");
                    foreach (DataRow row in data.Rows)
                    {
                        bool regExValid = ValidateDataEntry(row[i].ToString(), regExPattern);
                        if (!regExValid)
                        {
                            string newPattern = regExPattern.Replace("]", "_]");
                            if (!ValidateDataEntry(row[i].ToString(), newPattern))
                            {
                                invalidCount++;
                                Console.WriteLine(String.Format("Invalid: {0,-30} {1,-5} {2,-5}", row[i], data.Rows.IndexOf(row).ToString(), i.ToString()));
                            }
                        }
                    }
                }
                if (invalidCount == 0)
                {
                    Console.WriteLine("Data has passed validity checks. Ready for preprocessing.");
                }

            }
        }

        // Reads data dictionary csv file and populates dataDict attribute
        // params: data dictionary file name
        private void ReadDataDictionaryFile(string dataDictFilename)
        {
            dataDict = new DataTable();
            fileReadFailed = false;
            try
            {
                using (StreamReader sr = new StreamReader(dataDictFilename))
                {
                    string[] columnNames = sr.ReadLine().Split(',');
                    foreach (string columnName in columnNames)
                    {
                        dataDict.Columns.Add(columnName);
                    }

                    while (!sr.EndOfStream)
                    {
                        string[] record = sr.ReadLine().Split(',');
                        DataRow row = dataDict.NewRow();
                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            row[i] = record[i];
                        }
                        dataDict.Rows.Add(row);
                    }
                }
            }
            catch (Exception)
            {
                fileReadFailed = true;
                Console.WriteLine("Error when trying to read data dictionary file.");
            }
        }

        // Reads data csv file and populates data attribute
        // params: data filename
        private void ReadDataFile(string dataFilename)
        {
            data = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(dataFilename))
                {
                    // store fields
                    int colIndex = 0;
                    string[] fieldNames = sr.ReadLine().Split(',');
                    foreach (string fieldName in fieldNames)
                    {
                        string dataType = GetIntendedColumnDataType(dataDict, colIndex);
                        if (dataType == "Integer")
                        {
                            data.Columns.Add(fieldName, typeof(int));
                        }
                        else if (dataType == "String")
                        {
                            data.Columns.Add(fieldName, typeof(string));
                        }
                        colIndex++;
                        //Console.WriteLine(String.Format("{0,-20} {1,-5}", fieldName, data.Columns[fieldName].DataType.ToString()));
                    }
                    // store records
                    while (!sr.EndOfStream)
                    {
                        string[] record = sr.ReadLine().Split(',');
                        DataRow row = data.NewRow();
                        for (int i = 0; i < fieldNames.Length; i++)
                        {
                            // Levy Field contains '-' which should be read as 0
                            if (i == 2 && record[i] == "-")
                            {
                                row[i] = 0;
                            }
                            else
                            {
                                row[i] = record[i];
                            }
                        }
                        data.Rows.Add(row);
                    }
                }
            }
            catch (Exception)
            {
                fileReadFailed = true;
                Console.WriteLine("Error when trying to read dataset file.");
                Console.ReadLine();
                Environment.Exit(1);
            }
        }

        // Retrieves data type in row i of the data dictionary
        // params: data dictionary, row index of dictionary
        // returns: intended data type for the column
        private string GetIntendedColumnDataType(DataTable dataDict, int i)
        {
            string dataType = dataDict.Rows[i]["Data type"].ToString();
            return dataType;
        }

        // Validates data entry by checking it matches its corresponding regular expression pattern
        // params: input string, regular expression pattern
        // returns: boolean for whether the input string is valid or not
        private bool ValidateDataEntry(string value, string pattern)
        {
            bool isValid = Regex.IsMatch(value, pattern);
            if (isValid)
            {
                return true;
            }
            return false;
        }
    }
}
