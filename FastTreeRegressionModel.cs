using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.ML;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the implemenation of the Fast tree regression trainer provided by Microsoft.ML
    internal class FastTreeRegressionModel : RegressionModel
    {
        ITransformer model;
        MLContext mlContext;
        string trainDataPath;
        string testDataPath;
        List<string> xFeatures;
        int minSampleCountPerLeaf;
        public FastTreeRegressionModel(DataTable data) : base(data)
        {
            minSampleCountPerLeaf = 10;
        }
        
        public override void PreProcessData()
        {
            DataPreprocessor dp = new DataPreprocessor(data);
            // initial data checks
            dp.CheckForMissingValues();
            dp.RemoveDuplicateRecords();
            // convert data to numerical form
            dp.SplitEngineVolumeColumn();
            dp.MakeCategoryColumnNumerical();
            dp.MakeColourColumnNumerical();
            dp.MakeDriveWheelsColumnNumerical();
            dp.MakeFuelTypeColumnNumerical();
            dp.MakeGearBoxTypeColumnNumerical();
            dp.MakeLeatherInteriorColumnNumerical();
            dp.MakeMileageColumnNumerical();
            dp.MakeWheelColumnNumerical();

            // locate outliers
            DataUtilities.DisplayColumnNames(data);

            List<int> priceOutliers = dp.LocateOutliersWithZScores("Price", 3);
            DataUtilities.DisplayRows(data, priceOutliers.ToArray());
            data.Rows[priceOutliers[1]].Delete();

            List<int> levyOutliers = dp.LocateOutliersWithZScores("Levy", 5);

            List<int> prodYearOutliers = dp.LocateOutliersWithZScores("Prod. year", 3);
            DataUtilities.DisplayRows(data, prodYearOutliers.ToArray());
            int j = 0;
            foreach (int i in prodYearOutliers)
            {
                data.Rows[i - j].Delete();
                j++;
            }

            List<int> cylindersOutliers = dp.LocateOutliersWithZScores("Cylinders", 5);

            List<int> airbagsOutliers = dp.LocateOutliersWithZScores("Airbags", 3);

            List<int> engineDisplacementOutliers = dp.LocateOutliersWithZScores("Engine displacement", 6);
            dp.ImputeNumericalWithMean("Engine displacement", engineDisplacementOutliers[0]);

            List<int> mileageInKmOutliers = dp.LocateOutliersWithModifiedZScores("Mileage in km", 3.5);
            j = 0;
            foreach (int i in mileageInKmOutliers)
            {
                data.Rows[i - j].Delete();
                j++;
            }
            Console.ReadLine();

            // Manufacturers which appear less than 10 times given 'Other' value as there are not enough data points
            Dictionary<string,int> uniqueManufacturers = new Dictionary<string,int>();
            foreach (DataRow row in data.Rows)
            {
                if (uniqueManufacturers.ContainsKey(row["Manufacturer"].ToString()))
                {
                    uniqueManufacturers[row["Manufacturer"].ToString()]++;
                }
                else
                {
                    uniqueManufacturers.Add(row["Manufacturer"].ToString(), 1);
                }
            }
            int rowNo = 0;
            foreach (var kvp in uniqueManufacturers)
            {
                if (kvp.Value < 10)
                {
                    data.Rows[rowNo]["Manufacturer"] = "OTHER";
                }
                rowNo++;
            }


            // scale data
            dp.ScaleNumericalColumnStandardisation("Levy");
            dp.ScaleNumericalColumnStandardisation("Prod. year");
            dp.ScaleNumericalColumnStandardisation("Cylinders");
            dp.ScaleNumericalColumnStandardisation("Airbags");
            dp.ScaleNumericalColumnStandardisation("Engine displacement");
            dp.ScaleNumericalColumnStandardisation("Mileage in km");
        }

        public override void SelectFeatures()
        {
            // feature selection
            FeatureSelector fs = new FeatureSelector(trainingData);

            // display plots
            string[] continuousColumns = { "Levy scaled", "Prod. year scaled", "Cylinders scaled", "Airbags scaled", "Engine displacement scaled", "Mileage in km scaled" };
            List<string> binaryColumns = new List<string>();
            foreach (var column in data.Columns)
            {
                if (column.ToString().StartsWith("Is"))
                {
                    binaryColumns.Add(column.ToString());
                }
            }
            /*
            fs.DisplayBoxPlots(continuousColumns);
            fs.DisplayScatterPlot("Levy");
            fs.DisplayScatterPlot("Prod. year");
            fs.DisplayScatterPlot("Cylinders");
            fs.DisplayScatterPlot("Airbags");
            fs.DisplayScatterPlot("Engine displacement");
            fs.DisplayScatterPlot("Mileage in km");*/

            // correlation coefficients
            fs.RankNumericalColumnsByPearsonCoefficients(continuousColumns);
            fs.RankBinaryColumnsByPointBiserialCoefficients(binaryColumns.ToArray());
            Console.ReadLine();
        }
        protected override void Train()
        {
            trainDataPath = "prepped-training-data.csv";
            WriteDataToCsv(trainingFolds, trainDataPath); 
            mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromTextFile<CarData>(trainDataPath, hasHeader: true, separatorChar: ',');

            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Price")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "ManufacturerEncoded", inputColumnName: "Manufacturer"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "ModelEncoded", inputColumnName: "Model"))
                .Append(mlContext.Transforms.Concatenate("Features", xFeatures.ToArray())) // change predictors by passing the corresponding attributes
                .Append(mlContext.Regression.Trainers.FastTree(minimumExampleCountPerLeaf: minSampleCountPerLeaf));

            model = pipeline.Fit(dataView);
        }

        // Writes data to a csv file
        // params: data, file path
        private void WriteDataToCsv(DataTable data, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                // write the column names as the header row 
                sw.WriteLine(string.Join(",", data.Columns.Cast<DataColumn>().Select(col => col.ColumnName)));

                // write each row's values of data
                foreach (DataRow row in data.Rows)
                {
                    sw.WriteLine(String.Join(",", row.ItemArray));
                }
            }
        }
     
        protected override void Fit()
        {
            testDataPath = "validation-data.csv";
            WriteDataToCsv(validationFold, testDataPath); 

            IDataView dataView = mlContext.Data.LoadFromTextFile<CarData>(testDataPath, hasHeader: true, separatorChar: ',');

            var modelPredictions = model.Transform(dataView);

            var predictedValues = mlContext.Data.CreateEnumerable<CarPricePrediction>(modelPredictions, reuseRowObject: false);

            yPredictions = new double[validationFold.Rows.Count];
            int i = 0;
            foreach (var prediction in predictedValues)
            {
                yPredictions[i] = prediction.Price;
                i++;
            }

        }

        // Fits the training data (instead of validation) for the purpose of viewing if the decision tree overfits the training data
        public void FitTrainingData()
        {

            IDataView dataView = mlContext.Data.LoadFromTextFile<CarData>(trainDataPath, hasHeader: true, separatorChar: ',');

            var modelPredictions = model.Transform(dataView);

            var records = mlContext.Data.CreateEnumerable<CarData>(dataView, reuseRowObject: false);

            var predictedValues = mlContext.Data.CreateEnumerable<CarPricePrediction>(modelPredictions, reuseRowObject: false);

            int i = 0;
            List<double> preds = new List<double>();
            List<double> actual = new List<double>();
            foreach (var prediction in predictedValues)
            {
                preds.Add(prediction.Price);
                i++;
            }
            foreach (var record in records)
            {
                actual.Add(record.Price);
            }

            ModelValidator mv = new ModelValidator(preds.ToArray(), actual.ToArray());
            double mae = Math.Round(mv.CalculateMAE(),6);
            double rmse = Math.Round(mv.CalculateRMSE(),6);
            double r2 = Math.Round(mv.CalculateRSquared(),6);
            double adjR2 = Math.Round(mv.CalculateAdjustedRSquared(noOfPredictors), 6);
            Console.WriteLine($"{"Training",-12} {"",-16} {"",-20} {mae,-14} {rmse,-14} {r2,-14} {adjR2,-14}");
        }
        
        public override void EvaluateModelsToDecideOptimalFeatureSelection()
        {
            // decision trees prone to overffitting so I have implemented a comparison of the model's performance on the training data vs unseen validation data
            List<string> possibleFeatures = new List<string>() { "ProdYear", "MileageInKm", "EngineDisplacement", "ManufacturerEncoded", "ModelEncoded",
                "IsJeep", "IsDiesel", "IsTiptronic", "IsLeatherInterior", "IsEngineTurbo", "IsWheelLeft", "IsHatchback", "IsSedan", "IsManual", "Is4x4",
                "IsHybrid", "IsFront", "IsBlack", "IsSilver", "IsAutomatic", "IsGreen", "IsGoodsWagon", "IsLPG", "IsUniversal", "IsRed",
                "IsVariator", "IsPetrol" }; // ordered by feature importance

            Console.WriteLine("Table showing Fast tree regression trainer performance for a number of features:");
            Console.WriteLine($"{"Data",-12} {"No. of features",-16} {"Added feature", -20} {"MAE",-14} {"RMSE",-14} {"R-Sqaured",-14} {"Adj R-squared",-14}");
            // adds new feature each time
            for (int i=0; i<possibleFeatures.Count; i++)
            {
                xFeatures = possibleFeatures.Take(i + 1).ToList();
                noOfPredictors = xFeatures.Count;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{"Validation",-12} {xFeatures.Count,-16} {possibleFeatures[i],-20} {errorArray[0],-14} {errorArray[1],-14} {errorArray[2],-14} {errorArray[3],-14}");
                FitTrainingData();
            }
            
        }

        public override string FineTuneModel()
        {
            xFeatures = new List<string>() { "ProdYear", "MileageInKm", "EngineDisplacement", "ManufacturerEncoded", "ModelEncoded",
                "IsJeep", "IsDiesel", "IsTiptronic", "IsLeatherInterior", "IsEngineTurbo" };
            noOfPredictors = xFeatures.Count;
            int[] samplesPerLeaf = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            int optimalValue = 0;
            double bestRMSE = double.PositiveInfinity;
            double[] bestErrors = new double[4];

            Console.WriteLine("Fine-tuning the minimum sample count per leaf hyperparameter:");
            Console.WriteLine($"{"Min. samples per leaf",-24} {"MAE",-18} {"RMSE",-18} {"R-Squared",-18} {"Adj R-Squared",-18}");
            // tests different values for the samples per leaf
            foreach (int value in samplesPerLeaf)
            {
                minSampleCountPerLeaf = value;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{value,-24} {errorArray[0],-18} {errorArray[1],-18} {errorArray[2],-18} {errorArray[3],-18}");
                if (errorArray[1] < bestRMSE)
                {
                    bestRMSE = errorArray[1];
                    errorArray.CopyTo(bestErrors, 0);
                    optimalValue = value;
                }
            }

            string xFeaturesStr = String.Join(", ", xFeatures.ToArray());
            xFeaturesStr.TrimEnd(',', ' ');
            Console.WriteLine("Fast tree model conclusion written to file");
            return $"Microsft.ML Fast tree regression trainer: \n" +
                   $"Errors: MAE = {bestErrors[0]}, RMSE = {bestErrors[1]}, R-Squared = {bestErrors[2]}, Adjusted R-Squared = {bestErrors[3]} \n" +
                   $"Hyperparameters: Minimum sample count per leaf = {optimalValue} ***Inconclusive value - check with holdout data*** \n" +
                   $"Features: {xFeaturesStr}";
        }

        protected override void SetFinalHyperparameters()
        {
            minSampleCountPerLeaf = 8;
            xFeatures = new List<string>() { "ProdYear", "MileageInKm", "EngineDisplacement", "ManufacturerEncoded", "ModelEncoded",
                "IsJeep", "IsDiesel", "IsTiptronic", "IsLeatherInterior", "IsEngineTurbo" };
            noOfPredictors = 10;
        }

        // Uses holdout data to verify the optimal value for the number of samples per leaf hyperparameter
        public void DetermineHyperparameterWithHoldoutData()
        {
            trainingFolds = trainingData.Clone();
            trainingFolds.Merge(trainingData);
            trainingFolds.Merge(validationData);
            validationFold = holdoutData.Copy();
            xFeatures = new List<string>() { "ProdYear", "MileageInKm", "EngineDisplacement", "ManufacturerEncoded", "ModelEncoded",
                "IsJeep", "IsDiesel", "IsTiptronic", "IsLeatherInterior", "IsEngineTurbo" };
            noOfPredictors = xFeatures.Count;
            int[] samplesPerLeaf = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            int optimalValue = 0;
            double bestRMSE = double.PositiveInfinity;
            double[] bestErrors = new double[4];

            Console.WriteLine("Fine-tuning the minimum sample count per leaf hyperparameter:");
            Console.WriteLine($"{"Min. samples per leaf",-24} {"MAE",-18} {"RMSE",-18} {"R-Squared",-18} {"Adj R-Squared",-18}");
            // tests different values for the samples per leaf
            foreach (int value in samplesPerLeaf)
            {
                minSampleCountPerLeaf = value;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{value,-24} {errorArray[0],-18} {errorArray[1],-18} {errorArray[2],-18} {errorArray[3],-18}");
                if (errorArray[1] < bestRMSE)
                {
                    bestRMSE = errorArray[1];
                    errorArray.CopyTo(bestErrors, 0);
                    optimalValue = value;
                }
            }
        }

        // Saves trained model to zip file for UI integration
        public void SaveTrainedModelToFile()
        {
            minSampleCountPerLeaf = 8;
            trainingFolds = data;
            xFeatures = new List<string>() {"ProdYear", "MileageInKm", "EngineDisplacement", "ManufacturerEncoded", "ModelEncoded",
                "IsJeep", "IsDiesel", "IsTiptronic", "IsLeatherInterior", "IsEngineTurbo"};
            Train();
            mlContext.Model.Save(model, null, "trained-model.zip");
            Console.WriteLine("Trained fast tree model has been written to file: trained-model.zip");
        }

        // Saves the makes and models to a json file for UI integration
        public void SaveMakeAndModelToFile()
        {
            DataColumn makeColumn = data.Columns["Manufacturer"];
            DataColumn modelColumn = data.Columns["Model"];
            Dictionary<string, List<string>> makesAndModels = new Dictionary<string, List<string>>();
            foreach(DataRow row in data.Rows)
            {
                string makeModel = row["Manufacturer"].ToString() + " " + row["Model"].ToString();
                bool containsOnlyAscii = ContainsOnlyAsciiCharacters(makeModel);
                if (containsOnlyAscii && !Regex.IsMatch(makeModel, "_+"))
                {
                    string key = row["Manufacturer"].ToString();
                    if (!makesAndModels.ContainsKey(row["Manufacturer"].ToString()))
                    {
                        List<string> value = new List<string> { row["Model"].ToString() };
                        makesAndModels.Add(key, value);
                    }
                    else if (!makesAndModels[key].Contains(row["Model"].ToString()))
                    {
                        makesAndModels[key].Add(row["Model"].ToString());
                    }
                }
                else
                {
                    Console.WriteLine($"Did not write '{makeModel}' to file.");
                }
            }

            string json = JsonConvert.SerializeObject(makesAndModels, Formatting.Indented);

            string jsonFilePath = "make-model-data.json";
            File.WriteAllText(jsonFilePath, json);
            Console.WriteLine($"Make and model data has been written to file: {jsonFilePath}");
        }

        // Checks that a string contains only ascii characters
        // params: input string
        // returns: boolean for whether it contains ascii or not
        private bool ContainsOnlyAsciiCharacters(string value)
        {
            bool containsOnlyAscii = true;
            foreach (char c in value)
            {
                if (c > 127)
                {
                    containsOnlyAscii = false;
                }
            }
            return containsOnlyAscii;
        }
    }
}
