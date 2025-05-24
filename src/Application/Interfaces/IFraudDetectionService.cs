using Application.Entities;

namespace Application.Interfaces;

public interface IFraudDetectionService
{
    public TransactionPrediction Predict(TransactionData transaction);
}
