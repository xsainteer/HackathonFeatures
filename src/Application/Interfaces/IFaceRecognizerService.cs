using Azure.AI.Vision.Face;

namespace Application.Interfaces;

public interface IFaceRecognizerService
{
    public Task<List<Guid>> IdentifyFaces(BinaryData photoData);
    public Task<List<FaceDetectionResult>> DetectFacesAsync(BinaryData photoData);
    public Task AddPersonWithFaceAsync(string personName, BinaryData photoData);
}