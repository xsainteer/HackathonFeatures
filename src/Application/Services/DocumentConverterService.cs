    using System.Text;
    using Application.DTOs;
    using Application.Interfaces;
    using Azure.AI.DocumentIntelligence;
    using Domain.Enums;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Http;


    namespace Application.Services;
    
    public interface IDocumentConverterService
    {
        Task<ConvertedDocumentDto> ConvertDocumentAsync(IFormFile file, ExportFormat format);
    }
    
    public class DocumentConverterService : IDocumentConverterService
    {
        private readonly IDocumentIntelligenceService _documentIntelligenceService;
        private readonly ILogger<DocumentConverterService> _logger;

        public DocumentConverterService(IDocumentIntelligenceService documentIntelligenceService, ILogger<DocumentConverterService> logger)
        {
            _documentIntelligenceService = documentIntelligenceService;
            _logger = logger;
        }
        
        // not fully async, maybe rewrite it using different libraries
        // and uses IFormFile, web dependency
        public async Task<ConvertedDocumentDto> ConvertDocumentAsync(IFormFile file, ExportFormat format)
        {
            try
            {
                await using var fileStream = file.OpenReadStream();
                var binaryData = await BinaryData.FromStreamAsync(fileStream);
                
                var tables = await _documentIntelligenceService.GetObjectFromFormRecognizer(binaryData);
                
                var result = format switch
                {
                    ExportFormat.Csv => Encoding.UTF8.GetBytes(ConvertTablesToCsv(tables)),
                    ExportFormat.Excel => ConvertTablesToExcel(tables),
                    ExportFormat.Pdf => ConvertTablesToPdf(tables),
                    _ => throw new ArgumentOutOfRangeException(nameof(format), "Unsupported export format"),
                };
                
                var contentType = format switch
                {
                    ExportFormat.Csv => "text/csv",
                    ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExportFormat.Pdf => "application/pdf",
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
                };
                
                var fileExtension = format switch
                {
                    ExportFormat.Csv => "csv",
                    ExportFormat.Excel => "xlsx",
                    ExportFormat.Pdf => "pdf",
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
                };

                var convertedDocument = new ConvertedDocumentDto
                {
                    Content = result,
                    ContentType = contentType,
                    ContentExtension = fileExtension,
                };
                
                return convertedDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("error exporting tables: {Message}", e.Message);
                throw;
            }
        }
        
        private string ConvertTablesToCsv(IReadOnlyList<DocumentTable> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                foreach (var row in table.Cells.GroupBy(c => c.RowIndex))
                {
                    var cells = row.OrderBy(c => c.ColumnIndex)
                                   .Select(cell => EscapeForCsv(cell.Content));
                    sb.AppendLine(string.Join(",", cells));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private byte[] ConvertTablesToExcel(IReadOnlyList<DocumentTable> tables)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            int sheetIndex = 1;

            foreach (var table in tables)
            {
                var sheet = workbook.Worksheets.Add($"Table{sheetIndex++}");
                foreach (var row in table.Cells.GroupBy(c => c.RowIndex))
                {
                    foreach (var cell in row)
                    {
                        sheet.Cell(cell.RowIndex + 1, cell.ColumnIndex + 1).Value = cell.Content;
                    }
                }
            }

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        private byte[] ConvertTablesToPdf(IReadOnlyList<DocumentTable> tables)
        {
            using var ms = new MemoryStream();
            using var document = new PdfSharpCore.Pdf.PdfDocument();
            var page = document.AddPage();
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            var font = new PdfSharpCore.Drawing.XFont("Arial", 10);

            double y = 40;
            foreach (var table in tables)
            {
                foreach (var row in table.Cells.GroupBy(c => c.RowIndex))
                {
                    double x = 40;
                    foreach (var cell in row.OrderBy(c => c.ColumnIndex))
                    {
                        gfx.DrawString(cell.Content, font, PdfSharpCore.Drawing.XBrushes.Black, new PdfSharpCore.Drawing.XRect(x, y, 100, 20));
                        x += 100;
                    }
                    y += 20;
                    if (y > page.Height - 40)
                    {
                        page = document.AddPage();
                        gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
                        y = 40;
                    }
                }
                y += 20;
            }

            document.Save(ms);
            return ms.ToArray();
        }

        private string EscapeForCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            var escaped = value.Replace("\"", "\"\"");
            if (escaped.Contains(",") || escaped.Contains("\"") || escaped.Contains("\n"))
                return $"\"{escaped}\"";
            return escaped;
        }
    }