using Application.Interfaces;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace Infrastructure.Azure.FormRecognizer;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentIntelligenceClient _client;
    private readonly DocumentAnalysisClient _analysisClient;
    

    public DocumentIntelligenceService(DocumentIntelligenceClient client, DocumentAnalysisClient analysisClient)
    {
        _client = client;
        _analysisClient = analysisClient;
    }

    public async Task<object?> GetObjectFromFormRecognizer(Stream stream)
    {
        var operation = await _analysisClient.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
        
        var result = operation.Value;
        
        //TODO - Add logic to process the result and return the desired object
        return result;
    }
}