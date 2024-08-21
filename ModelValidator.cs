using System;

namespace RegressionAnalysisProj
{
    // Class that encapsulates the methods for calculating regression error metrics
    internal class ModelValidator
    {
        private double[] yPred;
        private double[] yActual;
        public ModelValidator(double[] yPredictions, double[] yActual)
        {
            yPred = yPredictions;
            this.yActual = yActual;
        }

        // Calculates the Mean Absolute Error (MAE)
        // returns: MAE
        public double CalculateMAE()
        {
            if (yActual.Length != yPred.Length)
            {
                throw new Exception("Error. Different number of actual and predicted values");
            }
            double sumOfAbsResiduals = 0;
            for (int i = 0; i < yActual.Length; i++)
            {
                sumOfAbsResiduals += Math.Abs(yActual[i] - yPred[i]);
            }
            double mae = sumOfAbsResiduals / yActual.Length;
            return mae;
        }

        // Calculates the Root Mean Square Error (RMSE)
        // returns: RMSE
        public double CalculateRMSE()
        {
            double rmse = Math.Sqrt(CalculateMSE());
            return rmse;
        }

        // Calculates the Mean Square Error (MSE)
        // returns: MSE
        private double CalculateMSE()
        {
            if (yActual.Length != yPred.Length)
            {
                throw new Exception("Error. Different number of actual and predicted values");
            }
            double sumOfSqrResiduals = 0;
            for (int i = 0; i < yActual.Length; i++)
            {
                sumOfSqrResiduals += Math.Pow(yActual[i] - yPred[i], 2);
            }
            double mse = sumOfSqrResiduals / yActual.Length;
            return mse;
        }

        // Calculates the coeffecition of determination (r2)
        // returns: r2
        public double CalculateRSquared()
        {
            if (yActual.Length != yPred.Length)
            {
                throw new Exception("Error. Different number of actual and predicted values");
            }
            double actualMean = Statistics.CalculateMean(yActual);
            double sumOfSqrResiduals = 0; 
            double totalVariance = 0;
            for (int i = 0; i < yActual.Length; i++)
            {
                sumOfSqrResiduals += Math.Pow(yActual[i] - yPred[i], 2);
                totalVariance += Math.Pow(yActual[i] - actualMean, 2);
            }
            double r2 = 1 - sumOfSqrResiduals / totalVariance;
            return r2;
        }

        // Calculates the Adjusted R-squared value
        // params: number of predictors
        // returns: adjusted r2
        public double CalculateAdjustedRSquared(int noOfPredictors)
        {
            if (yActual.Length != yPred.Length)
            {
                throw new Exception("Error. Different number of actual and predicted values");
            }
            double r2 = CalculateRSquared();
            int n = yActual.Length;
            double adjR2 = 1 - (1 - r2) * ((double)n - 1) / ((double)n - (double)noOfPredictors - 1);
            return adjR2;
        }

    }
}
