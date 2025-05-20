namespace Application.Interfaces;

public interface IDocumentIntelligenceService
{
    public Task<object?> GetObjectFromFormRecognizer(Stream stream);
}