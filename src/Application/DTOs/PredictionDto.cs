namespace Application.DTOs;

public class PredictionDto
{
    public bool IsFraud {get; set;}
    public float Probability {get; set;}
    public bool ShowTransactionToOperator {get; set;}
}