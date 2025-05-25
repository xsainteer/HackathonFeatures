using Application.Interfaces;
using Azure;
using Azure.AI.DocumentIntelligence;

namespace Infrastructure.Azure.FormRecognizer;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client;

    public DocumentIntelligenceService(DocumentIntelligenceClient client)
    {
        _client = client;
    }
    
    public async Task<IReadOnlyList<DocumentTable>> GetObjectFromFormRecognizer(BinaryData data)
    {
        var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", data);
        
        return operation.Value.Tables;
    }
}