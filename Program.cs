using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace RegressionAnalysisProj
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataValidator dv = new DataValidator("train.csv", "data-dictionary.csv");
            dv.ValidateData();

            DataPreprocessor dp1 = new DataPreprocessor(dv.GetData());
            dp1.MakeColourColumnNumerical();
            dp1.MakeCategoryColumnNumerical();
            dp1.MakeFuelTypeColumnNumerical();
            dp1.MakeDriveWheelsColumnNumerical();
            dp1.MakeGearBoxTypeColumnNumerical();
            Console.Write(dp1.data);

            DataTable originalData = dv.GetData();
            DataTable data1 = originalData.Copy();
            DataTable data2 = originalData.Copy();
            DataTable data3 = originalData.Copy();
            DataTable data4 = originalData.Copy();
            DataTable data5 = originalData.Copy();

            RegressionModel[] models = new RegressionModel[5];
            models[0] = new MeanTemplateModel(data1);
            models[1] = new SimpleLinearRegressionModel(data2);
            models[2] = new MultiLinearRegressionModel(data3);
            models[3] = new KNNRegressionModel(data4);
            models[4] = new FastTreeRegressionModel(data5);

            
            StreamWriter sw = new StreamWriter("regression-analysis-conclusion.txt");

            foreach (RegressionModel model in models)
            {
                model.PreProcessData();
                model.SplitData();
                model.SelectFeatures();
                model.EvaluateModelsToDecideOptimalFeatureSelection();
                string modelConclusion = model.FineTuneModel();
                sw.WriteLine(modelConclusion);
                sw.WriteLine();
            }
            sw.Close();

            /** Testing with holdout data **/

            List<string> holdoutDataConclusion = new List<string>();

            foreach (RegressionModel model in models)
            {
                model.PreProcessData();
                model.SplitData();
                string line = model.TestModelWithHoldoutData();
                holdoutDataConclusion.Add(line);
            }

            Console.WriteLine("Final model testing with holdout data:");
            Console.WriteLine($"{"Mean teplate model",-22} {holdoutDataConclusion[0]}");
            Console.WriteLine($"{"Simple linear model",-22} {holdoutDataConclusion[1]}");
            Console.WriteLine($"{"Multi linear model",-22} {holdoutDataConclusion[2]}");
            Console.WriteLine($"{"KNN model",-22} {holdoutDataConclusion[3]}");
            Console.WriteLine($"{"Fast tree model",-22} {holdoutDataConclusion[4]}");
            Console.ReadLine();

            /** Best performing model **/

            FastTreeRegressionModel treeModel = new FastTreeRegressionModel(originalData);
            treeModel.PreProcessData();
            treeModel.SplitData();
            treeModel.DetermineHyperparameterWithHoldoutData();
            treeModel.SaveTrainedModelToFile();
            treeModel.SaveMakeAndModelToFile();
            
            Console.ReadLine();
        }

    }
}
