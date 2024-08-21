using System;
using System.Data;
using System.Linq;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the methods used to alter the structure of the data
    internal class DataTableModifier
    {
        public DataTable data;
        
        public DataTableModifier(DataTable argData)
        {
            data = argData;
        }

        // Checks if there are duplicate rows and, if found, removes them
        public void RemoveDuplicateRecords()
        {
            int initialNoOfRows = data.Rows.Count;

            // removes ID column
            if (data.Columns.Contains("ID"))
            {
                data.Columns.Remove("ID");
            }

            var duplicates = data.AsEnumerable()
                .GroupBy(row => string.Join(",", row.ItemArray.Select(item => item.ToString())))
                .Where(group => group.Count() > 1); // assigns groups of duplicate rows to duplicates
            if (duplicates.Any())
            {
                foreach(var group in duplicates)
                {
                    foreach (var row in group.Skip(1)) // skip first row because need to keep one copy of duplicate
                    {
                        data.Rows.Remove(row);
                    }
                }
                int noOfDeletedRows = initialNoOfRows - data.Rows.Count;
                Console.WriteLine($"{noOfDeletedRows} Duplicate records removed.");
            }
            else
            {
                Console.WriteLine("No duplicate records found.");
            }
        }

        // Converts Leather interior column from string to integer, with boolean values
        // Stores 1 if the car has a leather interior, and 0 if not
        public void MakeLeatherInteriorColumnNumerical()
        {
            DataColumn newColumn = new DataColumn("Is leather interior", typeof(int));
            data.Columns.Add(newColumn);
            foreach (DataRow row in data.Rows)
            {
                int intValue = 0;
                string value = row["Leather interior"].ToString();
                if (value == "Yes")
                {
                    intValue = 1;
                }
                row["Is leather interior"] = intValue;
            }
            data.Columns.Remove("Leather interior");
            Console.WriteLine("Leathor interior column has been modified.");
        }

        // Splits Engine Volume column into a column containing the numerical volume
        // And a boolean column (of integer data type) which stores a 1 if the car has a turbo engine, and 0 if not
        public void SplitEngineVolumeColumn()
        {
            DataColumn newColumn1 = new DataColumn("Engine displacement", typeof(float));
            DataColumn newColumn2 = new DataColumn("Is engine turbo", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            foreach (DataRow row in data.Rows)
            {
                int isTurbo = 0;
                string value = row["Engine volume"].ToString();
                string[] substrings = value.Split(' ');
                float engineDisplacement = float.Parse(substrings[0]);
                if (substrings.Length == 2)
                {
                    if (substrings[1] == "Turbo")
                    {
                        isTurbo = 1;
                    }
                }
                row["Engine displacement"] = engineDisplacement;
                row["Is engine turbo"] = isTurbo;
            }
            data.Columns.Remove("Engine volume");
            Console.WriteLine("Engine volume column has been split.");
        }

        // Converts Mileage column from string to integer by removing taling 'km'
        public void MakeMileageColumnNumerical()
        {
            DataColumn newColumn = new DataColumn("Mileage in km", typeof(int));
            data.Columns.Add(newColumn);
            foreach (DataRow row in data.Rows)
            {
                row["Mileage in km"] = int.Parse(row["Mileage"].ToString().TrimEnd(' ', 'k', 'm'));
            }
            data.Columns.Remove("Mileage");
            Console.WriteLine("Mileage column has been modified.");
        }

        // Converts Wheel column from string to integer, with boolean values
        // Stores 1 if the car's steering wheel is on the left, and 0 if it's on the right
        public void MakeWheelColumnNumerical()
        {
            DataColumn newColumn = new DataColumn("Is wheel left", typeof(int));
            data.Columns.Add(newColumn);
            foreach (DataRow row in data.Rows)
            {
                int intValue = 0;
                string value = row["Wheel"].ToString();
                if (value == "Left wheel")
                {
                    intValue = 1;
                }
                row["Is wheel left"] = intValue;
            }
            data.Columns.Remove("Wheel");
            Console.WriteLine("Wheel column has been modified.");
        }

        // Splits Category column into multiple boolean columns (of integer data type)
        // Each category value has its own column with a 1 stored where the car's category is this value
        public void MakeCategoryColumnNumerical()
        {
            DataColumn newColumn1 = new DataColumn("Is cabriolet", typeof(int));
            DataColumn newColumn2 = new DataColumn("Is coupe", typeof(int));
            DataColumn newColumn3 = new DataColumn("Is goods wagon", typeof(int));
            DataColumn newColumn4 = new DataColumn("Is hatchback", typeof(int));
            DataColumn newColumn5 = new DataColumn("Is jeep", typeof(int));
            DataColumn newColumn6 = new DataColumn("Is limousine", typeof(int));
            DataColumn newColumn7 = new DataColumn("Is microbus", typeof(int));
            DataColumn newColumn8 = new DataColumn("Is minivan", typeof(int));
            DataColumn newColumn9 = new DataColumn("Is pickup", typeof(int));
            DataColumn newColumn10 = new DataColumn("Is sedan", typeof(int));
            DataColumn newColumn11 = new DataColumn("Is universal", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            data.Columns.Add(newColumn3);
            data.Columns.Add(newColumn4);
            data.Columns.Add(newColumn5);
            data.Columns.Add(newColumn6);
            data.Columns.Add(newColumn7);
            data.Columns.Add(newColumn8);
            data.Columns.Add(newColumn9);
            data.Columns.Add(newColumn10);
            data.Columns.Add(newColumn11);
            foreach (DataRow row in data.Rows)
            {
                row["Is cabriolet"] = row["Is coupe"] = row["Is goods wagon"] = row["Is hatchback"] = row["Is jeep"] = row["Is limousine"] = row["Is microbus"] = row["Is minivan"] = row["Is pickup"] = row["Is sedan"] = row["Is universal"] = 0;
                string categoryValue = row["Category"].ToString();
                switch (categoryValue)
                {
                    case "Cabriolet":
                        row["Is cabriolet"] = 1;
                        break;
                    case "Coupe":
                        row["Is coupe"] = 1;
                        break;
                    case "Goods wagon":
                        row["Is goods wagon"] = 1;
                        break;
                    case "Hatchback":
                        row["Is hatchback"] = 1;
                        break;
                    case "Jeep":
                        row["Is jeep"] = 1;
                        break;
                    case "Limousine":
                        row["Is limousine"] = 1;
                        break;
                    case "Microbus":
                        row["Is microbus"] = 1;
                        break;
                    case "Minivan":
                        row["Is minivan"] = 1;
                        break;
                    case "Pickup":
                        row["Is pickup"] = 1;
                        break;
                    case "Sedan":
                        row["Is sedan"] = 1;
                        break;
                    case "Universal":
                        row["Is universal"] = 1;
                        break;
                    default:
                        Console.WriteLine("There is no matching car category.");
                        Console.ReadLine();
                        break;
                }
            }
            data.Columns.Remove("Category");
            Console.WriteLine("Category column has been modified.");
        }

        // Splits Fuel type column into multiple boolean columns (of integer data type)
        // Each fuel type value has its own corresponding boolean column
        public void MakeFuelTypeColumnNumerical()
        {
            DataColumn newColumn1 = new DataColumn("Is CNG", typeof(int));
            DataColumn newColumn2 = new DataColumn("Is diesel", typeof(int));
            DataColumn newColumn3 = new DataColumn("Is hybrid", typeof(int));
            DataColumn newColumn4 = new DataColumn("Is hydrogen", typeof(int));
            DataColumn newColumn5 = new DataColumn("Is LPG", typeof(int));
            DataColumn newColumn6 = new DataColumn("Is petrol", typeof(int));
            DataColumn newColumn7 = new DataColumn("Is plug-in hybrid", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            data.Columns.Add(newColumn3);
            data.Columns.Add(newColumn4);
            data.Columns.Add(newColumn5);
            data.Columns.Add(newColumn6);
            data.Columns.Add(newColumn7);
            foreach (DataRow row in data.Rows)
            {
                row["Is CNG"] = row["Is diesel"] = row["Is hybrid"] = row["Is hydrogen"] = row["Is LPG"] = row["Is petrol"] = row["Is plug-in hybrid"] = 0;
                string fuelTypeValue = row["Fuel Type"].ToString();
                switch (fuelTypeValue)
                {
                    case "CNG":
                        row["Is CNG"] = 1;
                        break;
                    case "Diesel":
                        row["Is diesel"] = 1;
                        break;
                    case "Hybrid":
                        row["Is hybrid"] = 1;
                        break;
                    case "Hydrogen":
                        row["Is hydrogen"] = 1;
                        break;
                    case "LPG":
                        row["Is LPG"] = 1;
                        break;
                    case "Petrol":
                        row["Is petrol"] = 1;
                        break;
                    case "Plug-in Hybrid":
                        row["Is plug-in hybrid"] = 1;
                        break;
                }
            }
            data.Columns.Remove("Fuel type");
            Console.WriteLine("Fuel type column has been modified.");
        }

        // Splits Gear box type column into multiple boolean columns (of integer data type)
        // Each gear box type value has its own corresponding boolean column
        public void MakeGearBoxTypeColumnNumerical()
        {
            DataColumn newColumn1 = new DataColumn("Is automatic", typeof(int));
            DataColumn newColumn2 = new DataColumn("Is manual", typeof(int));
            DataColumn newColumn3 = new DataColumn("Is tiptronic", typeof(int));
            DataColumn newColumn4 = new DataColumn("Is variator", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            data.Columns.Add(newColumn3);
            data.Columns.Add(newColumn4);
            foreach (DataRow row in data.Rows)
            {
                row["Is automatic"] = row["Is manual"] = row["Is tiptronic"] = row["Is variator"] = 0;
                string gearBoxValue = row["Gear box type"].ToString();
                switch (gearBoxValue)
                {
                    case "Automatic":
                        row["Is automatic"] = 1;
                        break;
                    case "Manual":
                        row["Is manual"] = 1;
                        break;
                    case "Tiptronic":
                        row["Is tiptronic"] = 1;
                        break;
                    case "Variator":
                        row["Is variator"] = 1;
                        break;
                    default:
                        Console.WriteLine("There is no matching gear box type.");
                        Console.ReadLine();
                        break;

                }
            }
            data.Columns.Remove("Gear box type");
            Console.WriteLine("Gear box type column has been modified.");
        }

        // Splits Drive wheels column into multiple boolean columns (of integer data type)
        // Each drive wheels value has its own corresponding boolean column
        public void MakeDriveWheelsColumnNumerical()
        {
            DataColumn newColumn1 = new DataColumn("Is 4x4", typeof(int));
            DataColumn newColumn2 = new DataColumn("Is front", typeof(int));
            DataColumn newColumn3 = new DataColumn("Is rear", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            data.Columns.Add(newColumn3);
            foreach (DataRow row in data.Rows)
            {
                row["Is 4x4"] = row["Is front"] = row["Is rear"] = 0;
                string driveWheelsValue = row["Drive wheels"].ToString();
                switch (driveWheelsValue)
                {
                    case "4x4":
                        row["Is 4x4"] = 1;
                        break;
                    case "Front":
                        row["Is front"] = 1;
                        break;
                    case "Rear":
                        row["Is rear"] = 1;
                        break;
                    default:
                        Console.WriteLine("There is no matching drive wheels type.");
                        Console.ReadLine();
                        break;
                }
            }
            data.Columns.Remove("Drive wheels");
            Console.WriteLine("Drive wheels column has been modified.");
        }

        // Splits Colour column into multiple boolean columns (of integer data type)
        // Each colour value has its own corresponding boolean column
        public void MakeColourColumnNumerical()
        {
            DataColumn newColumn1 = new DataColumn("Is beige", typeof(int));
            DataColumn newColumn2 = new DataColumn("Is black", typeof(int));
            DataColumn newColumn3 = new DataColumn("Is blue", typeof(int));
            DataColumn newColumn4 = new DataColumn("Is brown", typeof(int));
            DataColumn newColumn5 = new DataColumn("Is carnelian red", typeof(int));
            DataColumn newColumn6 = new DataColumn("Is golden", typeof(int));
            DataColumn newColumn7 = new DataColumn("Is green", typeof(int));
            DataColumn newColumn8 = new DataColumn("Is grey", typeof(int));
            DataColumn newColumn9 = new DataColumn("Is orange", typeof(int));
            DataColumn newColumn10 = new DataColumn("Is pink", typeof(int));
            DataColumn newColumn11 = new DataColumn("Is purple", typeof(int));
            DataColumn newColumn12 = new DataColumn("Is red", typeof(int));
            DataColumn newColumn13 = new DataColumn("Is silver", typeof(int));
            DataColumn newColumn14 = new DataColumn("Is sky blue", typeof(int));
            DataColumn newColumn15 = new DataColumn("Is white", typeof(int));
            DataColumn newColumn16 = new DataColumn("Is yellow", typeof(int));
            data.Columns.Add(newColumn1);
            data.Columns.Add(newColumn2);
            data.Columns.Add(newColumn3);
            data.Columns.Add(newColumn4);
            data.Columns.Add(newColumn5);
            data.Columns.Add(newColumn6);
            data.Columns.Add(newColumn7);
            data.Columns.Add(newColumn8);
            data.Columns.Add(newColumn9);
            data.Columns.Add(newColumn10);
            data.Columns.Add(newColumn11);
            data.Columns.Add(newColumn12);
            data.Columns.Add(newColumn13);
            data.Columns.Add(newColumn14);
            data.Columns.Add(newColumn15);
            data.Columns.Add(newColumn16);
            foreach (DataRow row in data.Rows)
            {
                row["Is beige"] = row["Is blue"] = row["Is black"] = row["Is brown"] = row["Is carnelian red"] = row["Is golden"] =
                    row["Is green"] = row["Is grey"] = row["Is orange"] = row["Is pink"] = row["Is purple"] = row["Is red"] =
                    row["Is silver"] = row["Is sky blue"] = row["Is white"] = row["Is yellow"] = 0;
                string colourValue = row["Color"].ToString();
                switch (colourValue)
                {
                    case "Beige":
                        row["Is beige"] = 1;
                        break;
                    case "Blue":
                        row["Is blue"] = 1;
                        break;
                    case "Black":
                        row["Is black"] = 1;
                        break;
                    case "Brown":
                        row["Is brown"] = 1;
                        break;
                    case "Carnelian red":
                        row["Is carnelian red"] = 1;
                        break;
                    case "Golden":
                        row["Is golden"] = 1;
                        break;
                    case "Green":
                        row["Is green"] = 1;
                        break;
                    case "Grey":
                        row["Is grey"] = 1;
                        break;
                    case "Orange":
                        row["Is orange"] = 1;
                        break;
                    case "Pink":
                        row["Is pink"] = 1;
                        break;
                    case "Purple":
                        row["Is purple"] = 1;
                        break;
                    case "Red":
                        row["Is red"] = 1;
                        break;
                    case "Silver":
                        row["Is silver"] = 1;
                        break;
                    case "Sky blue":
                        row["Is sky blue"] = 1;
                        break;
                    case "White":
                        row["Is white"] = 1;
                        break;
                    case "Yellow":
                        row["Is yellow"] = 1;
                        break;
                    default:
                        Console.WriteLine("There is no matching colour.");
                        Console.ReadLine();
                        break;
                }
            }
            data.Columns.Remove("Color");
            Console.WriteLine("Color column has been modified.");
        }
    }
}
