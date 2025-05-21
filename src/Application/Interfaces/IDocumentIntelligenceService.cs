namespace Application.Interfaces;

public interface IDocumentIntelligenceService
{
    public Task<object?> GetObjectFromFormRecognizer(BinaryData data);
}