using System;
using System.Data;

namespace RegressionAnalysisProj
{
    // Class encapsulating the implementation of a mean template model, purely for comparisons with other models
    internal class MeanTemplateModel : RegressionModel
    {
        public MeanTemplateModel(DataTable data) : base(data) // calls RegressionModel constructor
        {
            noOfPredictors = 0;
        }
        public override void PreProcessData()
        {
            // empty
        }

        public override void SelectFeatures()
        {
            // empty
        }

        protected override void Train()
        {
            // empty
        }

        protected override void Fit()
        {
            double[] yActualValues = DataUtilities.GetColumnValuesAsDoubleArray(trainingFolds, "Price");
            double yMean = Statistics.CalculateMean(yActualValues);
            yPredictions = new double[validationFold.Rows.Count];
            for (int i = 0; i < yPredictions.Length; i++)
            {
                yPredictions[i] = yMean;
            }
        }

        public override void EvaluateModelsToDecideOptimalFeatureSelection()
        {
            // empty
        }
        public override string FineTuneModel()
        {
            double[] errorArray = CrossValidate();
            Console.WriteLine("Mean template model conclusion written to file");
            return $"Mean template model: \n" +
                   $"Errors: MAE = {errorArray[0]}, RMSE = {errorArray[1]}, R-Squared = {errorArray[2]} \n" +
                   $"Hyperparameters: None \n" +
                   $"Features: None";
        }

        protected override void SetFinalHyperparameters()
        {
            // empty
        }
    }
}
