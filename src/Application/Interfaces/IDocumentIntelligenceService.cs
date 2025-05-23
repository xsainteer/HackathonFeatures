using Azure.AI.DocumentIntelligence;

namespace Application.Interfaces;

public interface IDocumentIntelligenceService
{
    // not very clean, but it's just a MVP
    public Task<IReadOnlyList<DocumentTable>> GetObjectFromFormRecognizer(BinaryData data);
}