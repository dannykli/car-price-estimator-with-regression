using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class encapsulating the implementation of the multi linear regression model
    internal class MultiLinearRegressionModel : RegressionModel
    {
        private int maxNoOfEpochs;
        private double learningRate;
        private double convergenceThreshold;
        private double[] weights;
        private double intercept;
        private List<string> xFeatures;
        private int epochCount;
        private bool hasConverged;
        public MultiLinearRegressionModel(DataTable data) : base (data)
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
            data.Rows[priceOutliers[1]].Delete();

            List<int> levyOutliers = dp.LocateOutliersWithZScores("Levy", 5);

            List<int> prodYearOutliers = dp.LocateOutliersWithZScores("Prod. year", 5);

            List<int> cylindersOutliers = dp.LocateOutliersWithZScores("Cylinders", 5);

            List<int> airbagsOutliers = dp.LocateOutliersWithZScores("Airbags", 3);

            List<int> engineDisplacementOutliers = dp.LocateOutliersWithZScores("Engine displacement", 6);
            dp.ImputeNumericalWithMean("Engine displacement", engineDisplacementOutliers[0]);

            List<int> mileageInKmOutliers = dp.LocateOutliersWithModifiedZScores("Mileage in km", 3.5);
            int j = 0;
            foreach (int i in mileageInKmOutliers)
            {
                data.Rows[i - j].Delete();
                j++;
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
            maxNoOfEpochs = 1000;
            hasConverged = false;
            weights = new double[xFeatures.Count];
            intercept = 0;
            convergenceThreshold = 0.1;
            double previousCost = double.MaxValue;
            double currentCost = 0;

            // Gradient descent iterations
            for (int epoch = 0; epoch < maxNoOfEpochs; epoch++)
            {
                double[] yPredicts = GDPredict();
                currentCost = GDCalculateCost(yPredicts);

                if (Math.Abs(previousCost - currentCost) < convergenceThreshold)
                {
                    //Console.WriteLine($"Loop broken as change in MSE is sufficiently small after {epoch + 1} loops");
                    epochCount = epoch + 1;
                    hasConverged = true;
                    break;
                }

                double[] weightDerivatives = GDCalculateWeightDerivatives(yPredicts);
                double interceptDerivative = GDCalculateInterceptDerivative(yPredicts);
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] -= learningRate * weightDerivatives[i];
                }
                intercept -= learningRate * interceptDerivative;

                previousCost = currentCost;
            }
        }

        // Gets the temporary price predictions using the current weights 
        // returns: y predictions
        private double[] GDPredict()
        {
            List<double> yPredicts = new List<double>();
            foreach (DataRow row in trainingFolds.Rows)
            {
                double yPred = intercept;
                for (int i = 0; i < xFeatures.Count; i++)
                {
                    yPred += weights[i] * Convert.ToDouble(row[xFeatures[i]]);
                }
                yPredicts.Add(yPred);
            }
            return yPredicts.ToArray();
        }

        // Calculates the partial derivatives with respect to each weight
        // parmas: current y predictions
        // returns: partial derivatives w.r.t. weight
        private double[] GDCalculateWeightDerivatives(double[] yPredicts)
        {
            List<double> partialDerivatives = new List<double>();
            int noOfDataPoints = trainingFolds.Rows.Count;
            double sumOfProducts = 0;
            foreach (string feature in xFeatures)
            {
                for (int i=0; i<noOfDataPoints; i++)
                {
                    double residual = (Convert.ToDouble(trainingFolds.Rows[i]["Price"]) - yPredicts[i]);
                    sumOfProducts += Convert.ToDouble(trainingFolds.Rows[i][feature]) * residual;
                }
                partialDerivatives.Add(-(2 / (double)noOfDataPoints) * sumOfProducts);
            }
            return partialDerivatives.ToArray();
        }

        // Calculates the partial derivative with respect to the intercept
        // params: current y predictions
        // returns: partial derivative w.r.t intercept
        private double GDCalculateInterceptDerivative(double[] yPredicts)
        {
            int noOfDataPoints = trainingFolds.Rows.Count;
            double sumOfResiduals = 0;
            for (int i = 0; i < noOfDataPoints; i++)
            {
                sumOfResiduals += (Convert.ToDouble(trainingFolds.Rows[i]["Price"]) - yPredicts[i]);
            }
            return (-(2 / (double)noOfDataPoints) * sumOfResiduals);
        }

        // Calculates the current cost function (Mean Square Error)
        // params: y predictions
        // returns: current MSE
        private double GDCalculateCost(double[] yPredicts)
        {
            double sumOfSquaredResiduals = 0;
            for (int i=0; i<yPredicts.Length; i++)
            {
                double yActual = Convert.ToDouble(trainingFolds.Rows[i]["Price"]);
                double residual = yActual - yPredicts[i];
                sumOfSquaredResiduals += Math.Pow(residual, 2);
            }
            return sumOfSquaredResiduals / yPredicts.Length;
        }

        protected override void Fit()
        {
            yPredictions = new double[validationFold.Rows.Count];
            for (int i = 0; i < yPredictions.Length; i++)
            {
                yPredictions[i] = intercept;
                for (int j = 0; j < xFeatures.Count; j++)
                {
                   yPredictions[i] += weights[j] * Convert.ToDouble(validationFold.Rows[i][xFeatures[j]]);
                }
            }
        }

        public override void EvaluateModelsToDecideOptimalFeatureSelection()
        {
            List<string> possibleFeatures = new List<string>() { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic", "Is leather interior", "Is engine turbo", "Cylinders scaled" }; // ordered by feature importance

            Console.WriteLine("Table showing Multi Linear Gradient Descent regression model performance for a number of features:");
            Console.WriteLine($"{"Feature no.",-12} {"Added feature",-28} {"MAE",-14} {"RMSE",-14} {"R-Squared",-14} {"Adj R-Squared",-14} {"Epochs",-6}");
            // Adds a new predictor each time
            for (int i = 0; i < possibleFeatures.Count; i++)
            {
                xFeatures = possibleFeatures.Take(i + 1).ToList();
                noOfPredictors = xFeatures.Count;
                learningRate = 0.1;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{xFeatures.Count,-12} {possibleFeatures[i],-28} {errorArray[0],-14} {errorArray[1],-14} {errorArray[2],-14} {errorArray[3],-14} {epochCount,-6}");
            }
        }

        public override string FineTuneModel()
        {
            xFeatures = new List<string>() { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic" };
            noOfPredictors = xFeatures.Count;
            double[] possibleLearningRates = { 1, 0.75, 0.5, 0.25, 0.1 };
            double optimalLearningRate = 0;
            double bestEpochCount = double.PositiveInfinity;

            Console.WriteLine("Fine-tuning the learning rate hyperparameter:");
            Console.WriteLine($"{"Learning rate",-18} {"Number of epochs",-18}");
            foreach (double lr in possibleLearningRates)
            {
                learningRate = lr;
                trainingFolds = trainingData;
                Train();
                if (!hasConverged)
                {
                    Console.WriteLine($"{learningRate,-18} {"Not converged",-18}");
                }
                else
                {
                    Console.WriteLine($"{learningRate,-18} {epochCount,-18}");
                    if (epochCount < bestEpochCount)
                    {
                        optimalLearningRate = learningRate;
                        bestEpochCount = epochCount;
                    }
                }
            }
            learningRate = optimalLearningRate;
            double[] errorArray = CrossValidate();

            string xFeaturesStr = String.Join(", ", xFeatures.ToArray());
            xFeaturesStr.TrimEnd(',', ' ');
            Console.WriteLine("Multi linear model conclusion written to file");
            return $"Multi linear regression model using gradient descent: \n" +
                   $"Errors: MAE = {errorArray[0]}, RMSE = {errorArray[1]}, R-Squared = {errorArray[2]}, Adjusted R-Squared = {errorArray[3]} \n" +
                   $"Hyperparameters: Learning rate = {optimalLearningRate}, Max no. of epochs = {maxNoOfEpochs}, Convergence threshold = {convergenceThreshold} \n" +
                   $"Features: {xFeaturesStr}";
        }

        protected override void SetFinalHyperparameters()
        {
            learningRate = 0.5;
            xFeatures = new List<string>() { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled",
                "Is jeep", "Is diesel", "Is tiptronic" };
            noOfPredictors = 6;
        }
    }
}
