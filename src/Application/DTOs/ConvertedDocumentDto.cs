namespace Application.DTOs;

public class ConvertedDocumentDto
{
    public byte[] Content { get; set; }
    public string ContentType { get; set; }
    public string ContentExtension { get; set; }
}