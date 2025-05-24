using Microsoft.ML.Data;

namespace Application.Entities;

public class TransactionData
{
    [LoadColumn(0)] public float Amount;
    [LoadColumn(1)] public float TimeSinceLast;
    [LoadColumn(2)] public float LocationChange;
    [LoadColumn(3)] public float IsForeign;
    [LoadColumn(4)] public float DeviceChange;
    [LoadColumn(5)] public bool Label; // true = fraud
}

public class TransactionPrediction
{
    [ColumnName("PredictedLabel")] public bool Prediction;
    public float Score;
    public float Probability;
}