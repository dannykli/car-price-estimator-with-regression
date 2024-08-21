using System;
using System.Data;

namespace RegressionAnalysisProj
{
    // Abstract class that encapsulates the core components of developing a regression model
    internal abstract class RegressionModel
    {
        protected DataTable data; // attribute is passed by reference throughout 
        protected DataTable trainingData; // to be used for feature selection, train and fitting
        protected DataTable validationData; // to be used for model validation
        protected DataTable holdoutData; // to be used for final model evaluation
        protected DataTable trainingFolds;
        protected DataTable validationFold;
        protected double[] yPredictions;
        protected DataTable originalData;
        protected int noOfPredictors;
        public RegressionModel(DataTable data)
        {
            this.data = data;
            originalData = data.Copy();
        }
        // For preprocessing data, ready for feature selection and training
        abstract public void PreProcessData();
        public void SplitData()
        {
            DataTable[] dataArray = DataUtilities.SplitData(data, 0.72, 0.18); // ratios chosen according to k=5
            trainingData = dataArray[0];
            validationData = dataArray[1];
            holdoutData = dataArray[2];
        }

        // For performing tasks like visualising data and ranking features to enable feature selection
        abstract public void SelectFeatures();

        // For training the model using the given regression model's algorithm
        abstract protected void Train();

        // For predicting the validation car prices using the trained regression model
        abstract protected void Fit();

        // Cross-validates using k-fold validation and averages the four regression error metrics
        // returns: array of error values
        protected double[] CrossValidate()
        {
            // error metrics
            double maeSum = 0;
            double rmseSum = 0;
            double r2Sum = 0;
            double adjR2Sum = 0;
            // set k here
            int k = 5;
            DataTable[] dataArray = DataUtilities.DivideDataInto4(trainingData);
            DataTable[] dataSubSets = new DataTable[5];
            dataSubSets[0] = validationData;
            dataArray.CopyTo(dataSubSets, 1);
            trainingFolds = dataSubSets[0].Clone();
            validationFold = dataSubSets[0].Clone();
            for (int i = 0; i < k; i++)
            {
                trainingFolds.Clear();
                validationFold.Clear();
                for (int j = 0; j < k; j++)
                {
                    if (j == i)
                    {
                        validationFold.Merge(dataSubSets[j]);
                    }
                    else
                    {
                        trainingFolds.Merge(dataSubSets[j]);
                    }
                }
                Train();
                Fit();
                double[] yActual = DataUtilities.GetColumnValuesAsDoubleArray(validationFold, "Price");
                ModelValidator mv = new ModelValidator(yPredictions, yActual);
                maeSum += mv.CalculateMAE();
                rmseSum += mv.CalculateRMSE();
                r2Sum += mv.CalculateRSquared();
                adjR2Sum += mv.CalculateAdjustedRSquared(noOfPredictors);
            }
            double mae = Math.Round(maeSum / k, 6);
            double rmse = Math.Round(rmseSum / k, 6);
            double r2 = Math.Round(r2Sum / k, 6);
            double adjR2 = Math.Round(adjR2Sum / k, 6);
            double[] errorArray = { mae, rmse, r2, adjR2 };
            return errorArray;
        }

        // For training and validating the regression model with different features to determine optimal feature combination
        abstract public void EvaluateModelsToDecideOptimalFeatureSelection();

        // For fine-tuning any hyperparameters
        abstract public string FineTuneModel();

        // For setting the values of the final hyperparameter values
        abstract protected void SetFinalHyperparameters();

        // Tests the model's performance with holdout data
        // returns: string containing the model's errors
        public string TestModelWithHoldoutData()
        {
            trainingFolds = trainingData.Clone();
            trainingFolds.Merge(trainingData);
            trainingFolds.Merge(validationData);
            validationFold = holdoutData.Copy();
            SetFinalHyperparameters();
            Train();
            Fit();
            double[] yActual = DataUtilities.GetColumnValuesAsDoubleArray(validationFold, "Price");
            ModelValidator mv = new ModelValidator(yPredictions, yActual);
            return $"MAE: {Math.Round(mv.CalculateMAE(),6),-15} RMSE: {Math.Round(mv.CalculateRMSE(),6),-15} R-Squared: {Math.Round(mv.CalculateRSquared(),6),-15} Adj R-Squared: {Math.Round(mv.CalculateAdjustedRSquared(noOfPredictors),6),-15}";
        }
    }
}
