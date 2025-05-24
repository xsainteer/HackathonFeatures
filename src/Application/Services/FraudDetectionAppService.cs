using Application.DTOs;
using Application.Entities;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public interface IFraudDetectionAppService
{
    PredictionDto Predict(TransactionData input);
}

public class FraudDetectionAppService : IFraudDetectionAppService
{
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly ILogger<FraudDetectionAppService> _logger;

    public FraudDetectionAppService(IFraudDetectionService fraudDetectionService, ILogger<FraudDetectionAppService> logger)
    {
        _fraudDetectionService = fraudDetectionService;
        _logger = logger;
    }

    public PredictionDto Predict(TransactionData input)
    {
        try
        {
            var prediction = _fraudDetectionService.Predict(input);
            
            return new PredictionDto
            {
                Probability = prediction.Probability,
                IsFraud = prediction.Prediction
            };
        }
        catch (Exception e)
        {
            _logger.LogError("Error while predicting: {ErrorMessage}", e.Message);
            throw;
        }
    }
}