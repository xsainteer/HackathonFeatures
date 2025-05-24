using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class DocumentIntelligenceController : ControllerBase
{
    private readonly IDocumentConverterService _documentConverterService;
    private readonly ILogger<DocumentIntelligenceController> _logger;

    public DocumentIntelligenceController(IDocumentConverterService documentConverterService, ILogger<DocumentIntelligenceController> logger)
    {
        _documentConverterService = documentConverterService;
        _logger = logger;
    }

    [HttpPost("convert")]
    public async Task<IActionResult> ConvertDocumentAsync(IFormFile file, ExportFormat format)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var convertedDocument = await _documentConverterService.ConvertDocumentAsync(file, format);
            return File(convertedDocument.Content, convertedDocument.ContentType, convertedDocument.ContentExtension);
        }
        catch (Exception e)
        {
            _logger.LogError("error converting document to {Format} : {Error}", format, e);
            throw;
        }
    }
}