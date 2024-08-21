using Microsoft.ML.Data;

namespace RegressionAnalysisProj
{
    // Class containing all the car data attributes for Microsoft.ML implementation
    public class CarData 
    {
        [LoadColumn(0)]
        public float Price;

        [LoadColumn(1)]
        public float Levy;

        [LoadColumn(2)]
        public string Manufacturer;

        [LoadColumn(3)]
        public string Model;

        [LoadColumn(4)]
        public float ProdYear;

        [LoadColumn(5)]
        public float Cylinders;

        [LoadColumn(6)]
        public float Airbags;

        [LoadColumn(7)]
        public float EngineDisplacement;

        [LoadColumn(8)]
        public float IsEngineTurbo;

        [LoadColumn(9)]
        public float IsCabriolet;

        [LoadColumn(10)]
        public float IsCoupe;

        [LoadColumn(11)]
        public float IsGoodsWagon;

        [LoadColumn(12)]
        public float IsHatchback;

        [LoadColumn(13)]
        public float IsJeep;

        [LoadColumn(14)]
        public float IsLimousine;

        [LoadColumn(15)]
        public float IsMicrobus;

        [LoadColumn(16)]
        public float IsMinivan;

        [LoadColumn(17)]
        public float IsPickup;

        [LoadColumn(18)]
        public float IsSedan;

        [LoadColumn(19)]
        public float IsUniversal;

        [LoadColumn(20)]
        public float IsBeige;

        [LoadColumn(21)]
        public float IsBlack;

        [LoadColumn(22)]
        public float IsBlue;

        [LoadColumn(23)]
        public float IsBrown;

        [LoadColumn(24)]
        public float IsCarnelianRed;

        [LoadColumn(25)]
        public float IsGolden;

        [LoadColumn(26)]
        public float IsGreen;

        [LoadColumn(27)]
        public float IsGrey;

        [LoadColumn(28)]
        public float IsOrange;

        [LoadColumn(29)]
        public float IsPink;

        [LoadColumn(30)]
        public float IsPurple;

        [LoadColumn(31)]
        public float IsRed;

        [LoadColumn(32)]
        public float IsSilver;

        [LoadColumn(33)]
        public float IsSkyBlue;

        [LoadColumn(34)]
        public float IsWhite;

        [LoadColumn(35)]
        public float IsYellow;

        [LoadColumn(36)]
        public float Is4x4;

        [LoadColumn(37)]
        public float IsFront;

        [LoadColumn(38)]
        public float IsRear;

        [LoadColumn(39)]
        public float IsCNG;

        [LoadColumn(40)]
        public float IsDiesel;

        [LoadColumn(41)]
        public float IsHybrid;

        [LoadColumn(42)]
        public float IsHydrogen;

        [LoadColumn(43)]
        public float IsLPG;

        [LoadColumn(44)]
        public float IsPetrol;

        [LoadColumn(45)]
        public float IsPlugInHybrid;

        [LoadColumn(46)]
        public float IsAutomatic;

        [LoadColumn(47)]
        public float IsManual;

        [LoadColumn(48)]
        public float IsTiptronic;

        [LoadColumn(49)]
        public float IsVariator;

        [LoadColumn(50)]
        public float IsLeatherInterior;

        [LoadColumn(51)]
        public float MileageInKm;

        [LoadColumn(52)]
        public float IsWheelLeft;

        [LoadColumn(53)]
        public float LevyScaled;

        [LoadColumn(54)]
        public float ProdYearScaled;

        [LoadColumn(55)]
        public float CylindersScaled;

        [LoadColumn(56)]
        public float AirbagsScaled;

        [LoadColumn(57)]
        public float EngineDisplacementScaled;

        [LoadColumn(58)]
        public float MileageInKmScaled;

    }

    // Class containing the price prediction atribute for Microsoft.ML implementation
    public class CarPricePrediction
    {
        [ColumnName("Score")]
        public float Price;
    }
}
