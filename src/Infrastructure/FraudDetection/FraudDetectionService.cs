using Application.Entities;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Infrastructure.FraudDetection;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly MLContext _mlContext;  
    private PredictionEngine<TransactionData, TransactionPrediction>? _predictionEngine;
    private const string ModelPath = "fraudModel.zip";
    private const string DataPath = "data/fraud_data.csv";
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(ILogger<FraudDetectionService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);

        if (File.Exists(ModelPath))
        {
            LoadModel();
        }
        else
        {
            TrainAndSaveModel();
            LoadModel();
        }
    }

    private void LoadModel()
    {
        try
        {
            ITransformer mlModel = _mlContext.Model.Load(ModelPath, out var modelInputSchema);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(mlModel);
            _logger.LogInformation("Loaded model from {ModelPath}", ModelPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load model from {ModelPath}", ModelPath);
            throw;
        }
    }

    private void TrainAndSaveModel()
    {
        try
        {
            if (!File.Exists(DataPath))
                throw new FileNotFoundException("Training data file not found at path: {DataPath}", DataPath);

            IDataView trainingDataView = _mlContext.Data.LoadFromTextFile<TransactionData>(
                path: DataPath,
                hasHeader: true,
                separatorChar: ',');

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(TransactionData.Amount),
                    nameof(TransactionData.TimeSinceLast),
                    nameof(TransactionData.LocationChange),
                    nameof(TransactionData.IsForeign),
                    nameof(TransactionData.DeviceChange))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: nameof(TransactionData.Label), featureColumnName: "Features"));

            _logger.LogInformation("Training data file: {DataPath}", DataPath);
            var model = pipeline.Fit(trainingDataView);
            _logger.LogInformation("Data file trained: {DataPath}", DataPath);

            _mlContext.Model.Save(model, trainingDataView.Schema, ModelPath);
            _logger.LogInformation("Model saved to file: {ModelPath}", ModelPath);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while training and saving model: {ErrorMessage}", e.Message);
            throw;
        }
    }

    public TransactionPrediction Predict(TransactionData input)
    {
        try
        {
            if (_predictionEngine == null)
            {
                _logger.LogError("No prediction engine.");
                throw new InvalidOperationException("Model is not loaded.");
            }

            return _predictionEngine.Predict(input);
        }
        catch (Exception e)
        {
            _logger.LogError("Error when predicting: {Error}", e.Message);
            throw;
        }
    }
}
