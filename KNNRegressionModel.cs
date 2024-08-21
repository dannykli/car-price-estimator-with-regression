using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the implemenatation of the KNN regression model
    internal class KNNRegressionModel : RegressionModel
    {
        List<string> xFeatures;
        private int k;
        public KNNRegressionModel(DataTable data) : base(data)
        {

        }
        
        public override void PreProcessData()
        {
            DataPreprocessor dp = new DataPreprocessor(data);
            // initial data checks
            dp.CheckForMissingValues();
            dp.RemoveDuplicateRecords();
            // convert data to numerical form
            dp.SplitEngineVolumeColumn();
            dp.MakeMileageColumnNumerical();
            dp.MakeCategoryColumnNumerical();
            dp.MakeColourColumnNumerical();
            dp.MakeDriveWheelsColumnNumerical();
            dp.MakeFuelTypeColumnNumerical();
            dp.MakeGearBoxTypeColumnNumerical();
            dp.MakeLeatherInteriorColumnNumerical();
            dp.MakeWheelColumnNumerical();
            // locate outliers
            DataUtilities.DisplayColumnNames(data);

            List<int> priceOutliers = dp.LocateOutliersWithZScores("Price", 3);
            data.Rows[priceOutliers[1]].Delete();

            List<int> levyOutliers = dp.LocateOutliersWithZScores("Levy", 5);

            List<int> prodYearOutliers = dp.LocateOutliersWithZScores("Prod. year", 5);

            List<int> cylindersOutliers = dp.LocateOutliersWithZScores("Cylinders", 5);

            List<int> airbagsOutliers = dp.LocateOutliersWithZScores("Airbags", 3);

            List<int> engineDisplacementOutliers = dp.LocateOutliersWithZScores("Engine displacement", 6);
            DataUtilities.DisplayRows(data, engineDisplacementOutliers.ToArray());
            dp.ImputeNumericalWithMean("Engine displacement", engineDisplacementOutliers[0]);

            List<int> mileageInKmOutliers = dp.LocateOutliersWithModifiedZScores("Mileage in km", 3);
            int j = 0;
            foreach (int i in mileageInKmOutliers)
            {
                data.Rows[i - j].Delete();
                j++;
            }

            // scale data
            dp.ScaleNumericalColumnMinMax("Levy");
            dp.ScaleNumericalColumnStandardisation("Prod. year");
            dp.ScaleNumericalColumnStandardisation("Cylinders");
            dp.ScaleNumericalColumnStandardisation("Airbags");
            dp.ScaleNumericalColumnStandardisation("Engine displacement");
            dp.ScaleNumericalColumnStandardisation("Mileage in km");

        }

        public override void SelectFeatures()
        {
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
            // empty - not really any training process for KNN
        }

        protected override void Fit()
        {
            yPredictions = new double[validationFold.Rows.Count];
            // iterates through each validation query point
            for (int i = 0; i < validationFold.Rows.Count; i++)
            {
                IndexedDistance[] distances = new IndexedDistance[trainingFolds.Rows.Count];
                // iterates through each data point and calculates a distance from the query point
                for (int trainingRowNo = 0; trainingRowNo < distances.Length; trainingRowNo++)
                {
                    double distance = CalculateDistance(validationFold.Rows[i], trainingFolds.Rows[trainingRowNo]);
                    IndexedDistance indexedDistance = new IndexedDistance(trainingRowNo, distance);
                    distances[trainingRowNo] = indexedDistance;
                }

                BubbleSortDescending(distances);
                int[] kNeighbours = new int[k];
                int kIndex = 0;
                for (int j = distances.Length - 1; j >= distances.Length - k; j--)
                {
                    kNeighbours[kIndex] = distances[j].Index;
                    kIndex++;
                }

                double ySum = 0;
                foreach(int neighbour in kNeighbours)
                {
                    ySum += Convert.ToDouble(trainingFolds.Rows[neighbour]["Price"]);
                }
                yPredictions[i] = ySum / k; // prediction is the average of the k nearest neighbours
            }
        }

        // Nested class for sorting the distances whilst keeping track of the data point that the distance is associated with
        private class IndexedDistance
        {
            public int Index { get; set; }
            public double Distance { get; set; }

            public IndexedDistance(int index, double distance)
            {
                Index = index;
                Distance = distance;
            }
        }

        // Calculates the distance between two data points (rows)
        // params: data row of query point, data row of data point
        // returns: euclidean distance
        private double CalculateDistance(DataRow row1, DataRow row2)
        {
            double squaredSum = 0;
            foreach (string feature in xFeatures)
            {
                double difference = Convert.ToDouble(row1[feature]) - Convert.ToDouble(row2[feature]);
                squaredSum += Math.Pow(difference, 2);
            }
            return Math.Sqrt(squaredSum);
        }

        // Sorts the k smallest distances by performing k bubble sort passes
        // params: array of distances
        // returns: sorted array of distances
        private IndexedDistance[] BubbleSortDescending(IndexedDistance[] distances)
        {
            for (int i=0; i < k; i++)
            {
                for (int j=0; j < distances.Length-i-1; j++)
                {
                    if (distances[j].Distance < distances[j + 1].Distance)
                    {
                        IndexedDistance temp = distances[j];
                        distances[j] = distances[j + 1];
                        distances[j + 1] = temp;
                    }
                }
            }
            return distances;
        }

        // Used for testing the modified bubble sort
        public void BubbleSortTest()
        {
            IndexedDistance[] testArray = new IndexedDistance[10];
            Random random = new Random();
            for (int i= 0; i < 10; i++)
            {
                int num = random.Next(1, 10);
                IndexedDistance a = new IndexedDistance(i, num);
                testArray[i] = a;
            }
            BubbleSortDescending(testArray);
        }

        public override void EvaluateModelsToDecideOptimalFeatureSelection()
        {
            k = 10;
            List<string> possibleFeatures = new List<string>() { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic", "Is leather interior", "Is engine turbo", "Cylinders scaled" }; // ordered by feature importance

            Console.WriteLine("Table showing KNN regression model performance for a number of features:");
            Console.WriteLine($"{"No. of features",-15} {"Added feature",-32} {"MAE",-18} {"RMSE",-18} {"R-Squared",-18} {"Adj R-Squared",-18}");
            // adds a new feature each time
            for (int i = 0; i < possibleFeatures.Count; i++)
            {
                xFeatures = possibleFeatures.Take(i + 1).ToList();
                noOfPredictors = xFeatures.Count;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{xFeatures.Count,-15} {possibleFeatures[i],-32} {errorArray[0],-18} {errorArray[1],-18} {errorArray[2],-18} {errorArray[3],-18}");
            }
        }

        public override string FineTuneModel()
        {
            xFeatures = new List<string>(){ "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic" };
            double bestRMSE = double.PositiveInfinity;
            double[] bestErrors = new double[4];
            double optimalK = 0;
            int[] kValues = new int[] { 5, 10, 20, 30, 40, 50 };

            Console.WriteLine("Fine-tuning the k value hyperparameter:");
            Console.WriteLine($"{"K value",-10} {"MAE",-18} {"RMSE",-18} {"R-Squared",-18} {"Adj R-Squared",-18}");
            // test different k values
            for (int i = 0; i < kValues.Length; i++)
            {
                k = kValues[i];
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{k,-10} {errorArray[0],-18} {errorArray[1],-18} {errorArray[2],-18} {errorArray[3],-18}");
                if (errorArray[1] < bestRMSE)
                {
                    bestRMSE = errorArray[1];
                    errorArray.CopyTo(bestErrors, 0);
                    optimalK = k;
                }
            }

            string xFeaturesStr = String.Join(", ",xFeatures.ToArray());
            xFeaturesStr.TrimEnd(',', ' ');
            Console.WriteLine("KNN model conclusion written to file");
            return $"K-nearest neighbours regression model: \n" +
                   $"Errors: MAE = {bestErrors[0]}, RMSE = {bestErrors[1]}, R-Squared = {bestErrors[2]}, Adjusted R-Squared = {bestErrors[3]} \n" +
                   $"Hyperparameters: K = {optimalK} \n" +
                   $"Features: {xFeaturesStr}";
        }

        protected override void SetFinalHyperparameters()
        {
            k = 10;
            xFeatures = new List<string>() { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic" };
            noOfPredictors = 6;
        }
    }
}
