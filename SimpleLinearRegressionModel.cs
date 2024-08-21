using System;
using System.Collections.Generic;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class encapsulating the implementation of the simple linear regression model
    internal class SimpleLinearRegressionModel : RegressionModel
    {
        private string xFeature;
        private double m;
        private double b;

        public SimpleLinearRegressionModel(DataTable data) : base(data) // calls RegressionModel constructor
        {
            noOfPredictors = 1;
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
            // locate outliers
            DataUtilities.DisplayColumnNames(data);

            List<int> priceOutliers = dp.LocateOutliersWithZScores("Price", 3);
            data.Rows[priceOutliers[1]].Delete();

            List<int> levyOutliers = dp.LocateOutliersWithZScores("Levy", 5);

            List<int> prodYearOutliers = dp.LocateOutliersWithZScores("Prod. year", 5);

            List<int> cylindersOutliers = dp.LocateOutliersWithZScores("Cylinders", 5);

            List<int> airbagsOutliers = dp.LocateOutliersWithZScores("Airbags", 3);

            dp.LocateOutliersWithIQR("Airbags");

            List<int> engineDisplacementOutliers = dp.LocateOutliersWithZScores("Engine displacement", 6);
            DataUtilities.DisplayRows(data, engineDisplacementOutliers.ToArray());
            Console.ReadLine();
            dp.ImputeNumericalWithMean("Engine displacement", engineDisplacementOutliers[0]);

            List<int> mileageInKmOutliers = dp.LocateOutliersWithModifiedZScores("Mileage in km", 3);
            int j = 0;
            foreach (int i in mileageInKmOutliers)
            {
                data.Rows[i-j].Delete();
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
            // perform feature selection on training data
            FeatureSelector fs = new FeatureSelector(trainingData);

            // display plots
            string[] columnNames = {"Levy scaled", "Prod. year scaled", "Cylinders scaled", "Airbags scaled", "Engine displacement scaled", "Mileage in km scaled" };
            fs.DisplayBoxPlots(columnNames);
            fs.DisplayScatterPlot("Levy");
            fs.DisplayScatterPlot("Prod. year");
            fs.DisplayScatterPlot("Cylinders");
            fs.DisplayScatterPlot("Airbags");
            fs.DisplayScatterPlot("Engine displacement");
            fs.DisplayScatterPlot("Mileage in km");

            // correlation coefficients
            fs.RankNumericalColumnsByPearsonCoefficients(columnNames);
        }

        protected override void Train()
        {
            double[] xTrain = DataUtilities.GetColumnValuesAsDoubleArray(trainingFolds, xFeature);
            double[] yTrain = DataUtilities.GetColumnValuesAsDoubleArray(trainingFolds, "Price");
            double m = Statistics.CalculateCoVariance(xTrain, yTrain) / Statistics.CalculateVariance(xTrain);
            double b = Statistics.CalculateMean(yTrain) - m * Statistics.CalculateMean(xTrain);
            this.m = m;
            this.b = b;
        }

        protected override void Fit()
        {
            double[] xValidation = DataUtilities.GetColumnValuesAsDoubleArray(validationFold, xFeature);
            yPredictions = new double[xValidation.Length];
            for (int i=0; i < yPredictions.Length; i++)
            {
                yPredictions[i] = m * xValidation[i] + b;
            }
        }

        public override void EvaluateModelsToDecideOptimalFeatureSelection()
        {
            string[] xFeatures = { "Prod. year scaled", "Mileage in km scaled", "Engine displacement scaled", "Cylinders scaled", "Levy scaled", "Airbags scaled" };
            Console.WriteLine("Table showing simple linear model performance for each feature:");
            Console.WriteLine($"{"Feature",-30} {"MAE",-20} {"RMSE",-20} {"R-Squared",-20}");
            // tests the model with a different feature each time
            foreach (string feature in xFeatures)
            {
                xFeature = feature;
                double[] errorArray = CrossValidate();
                Console.WriteLine($"{feature,-30} {errorArray[0],-20} {errorArray[1],-20} {errorArray[2],-20}");
            }
            Console.ReadLine();
        }

        public override string FineTuneModel()
        {
            xFeature = "Prod. year scaled";
            double optimalThreshold = 0;
            double bestRMSE = double.PositiveInfinity;
            double[] bestErrors = new double[4];
            double[] zScoreThresholdValues = { 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10 };
            DataTable preppedData = originalData.Copy();

            DataPreprocessor dp = new DataPreprocessor(preppedData);
            dp.RemoveDuplicateRecords();
            List<int> priceOutliers = dp.LocateOutliersWithZScores("Price", 3);
            preppedData.Rows[priceOutliers[1]].Delete();

            // tests the model with different z-score threshold values by instantiating a new model each iteration
            foreach (double value in zScoreThresholdValues)
            {
                DataTable tempData = preppedData.Copy();
                DataPreprocessor tempDp = new DataPreprocessor(tempData);
                SimpleLinearRegressionModel tempLinearModel = new SimpleLinearRegressionModel(tempData);
                tempLinearModel.xFeature = "Prod. year";

                List<int> prodYearOutliers = tempDp.LocateOutliersWithZScores("Prod. year", value);
                int j = 0;
                foreach (int i in prodYearOutliers)
                {
                    tempData.Rows[i - j].Delete();
                    j++;
                }

                tempLinearModel.SplitData();
                double[] errorArray = tempLinearModel.CrossValidate();
                double rmse = errorArray[1];
                
                if (rmse < bestRMSE)
                {
                    bestRMSE = rmse;
                    errorArray.CopyTo(bestErrors,0);
                    optimalThreshold = value;
                }
                Console.WriteLine($"RMSE: {rmse,-20} Threshold value: {value}");
            }
            Console.WriteLine("Simple linear model conclusion written to file");
            return $"Simple linear regression model: \n" +
                   $"Errors: MAE = {bestErrors[0]}, RMSE = {bestErrors[1]}, R-Squared = {bestErrors[2]} \n" +
                   $"Hyperparameters: Z-score threshold for Prod. Year outliers = {optimalThreshold} \n" +
                   $"Features: {xFeature}";
        }

        protected override void SetFinalHyperparameters()
        {
            DataPreprocessor dp = new DataPreprocessor(data);
            dp.LocateOutliersWithZScores("Prod. year", 4);
            xFeature = "Prod. year scaled";
            noOfPredictors = 1;
        }
    }
}
