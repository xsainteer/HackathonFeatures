using Application.Interfaces;
using Azure;
using Azure.AI.Vision.Face;
using Infrastructure.Azure.BlobStorage;
using Microsoft.Extensions.Options;

namespace Infrastructure.Azure.FaceRecognizer;

public class FaceRecognizerService : IFaceRecognizerService
{
    private readonly BlobStorageSettings _blobStorageSettings;
    private readonly FaceClient _faceClient;
    private readonly FaceRecognitionModel _faceRecognitionModel = FaceRecognitionModel.Recognition04;
    private readonly LargePersonGroupClient _largePersonGroupClient;
    private readonly FaceRecognizerSettings _faceRecognizerSettings;
    
    public FaceRecognizerService(
        IOptions<BlobStorageSettings> blobStorageSettings, 
        FaceClient faceClient,
        LargePersonGroupClient largePersonGroupClient, 
        IOptions<FaceRecognizerSettings> faceRecognizerSettings)
    {
        _faceClient = faceClient;
        _largePersonGroupClient = largePersonGroupClient;
        _faceRecognizerSettings = faceRecognizerSettings.Value;
        _blobStorageSettings = blobStorageSettings.Value;
    }

    public async Task<List<Guid>> IdentifyFacesAsync(BinaryData photoData)
    {
        await CreatePersonGroupIfNotExistsAsync();
        var detectedFaces = await DetectFacesAsync(photoData);
        
        var faceIds = detectedFaces.Select(f => f.FaceId.Value).ToList();

        if (faceIds.Count == 0)
        {
            Console.WriteLine("No faces detected.");
            return [];
        }
        
        var identifyResponse = await _faceClient.IdentifyFromLargePersonGroupAsync(
            faceIds,
            _faceRecognizerSettings.PersonGroupId);
        
        var identifyResults = identifyResponse.Value;
        
        var matchingPersonIds = new List<Guid>();
        
        foreach (FaceIdentificationResult identifyResult in identifyResults)
        {
            if (identifyResult.Candidates.Count == 0)
            {
                Console.WriteLine($"No candidates detected");
                continue;
            }

            FaceIdentificationCandidate candidate = identifyResult.Candidates[0];
            var getPersonResponse = await _largePersonGroupClient.GetPersonAsync(candidate.PersonId);
            string personName = getPersonResponse.Value.Name;
            Console.WriteLine($"Person '{personName}' is identified," + $" confidence: {candidate.Confidence}.");

            var verifyResponse = await _faceClient.VerifyFromLargePersonGroupAsync(identifyResult.FaceId, _faceRecognizerSettings.PersonGroupId, candidate.PersonId);
            FaceVerificationResult verifyResult = verifyResponse.Value;
            Console.WriteLine($"Verification result: is a match? {verifyResult.IsIdentical}. confidence: {verifyResult.Confidence}");
            
            if (verifyResult.IsIdentical)
            {
                matchingPersonIds.Add(candidate.PersonId);
            }
        }
        
        return matchingPersonIds;
    }
    
    public async Task<List<FaceDetectionResult>> DetectFacesAsync(BinaryData photoData)
    {
        var response = await _faceClient.DetectAsync(
            photoData, 
            FaceDetectionModel.Detection03,
            _faceRecognitionModel, 
            true,
            [FaceAttributeType.QualityForRecognition] );

        var detectedFaces = response.Value;
        
        List<FaceDetectionResult> sufficientQualityFaces = [];
        
        foreach (FaceDetectionResult detectedFace in detectedFaces)
        {
            QualityForRecognition? faceQualityForRecognition = detectedFace.FaceAttributes.QualityForRecognition;
            if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value != QualityForRecognition.Low))
            {
                sufficientQualityFaces.Add(detectedFace);
            }
        }
        
        return sufficientQualityFaces;
    }

    public async Task AddPersonWithFaceAsync(string personName, BinaryData photoData)
    {
        await CreatePersonGroupIfNotExistsAsync();
        var createdPersonResponse = await _largePersonGroupClient.CreatePersonAsync(personName);
        
        var personId = createdPersonResponse.Value.PersonId;
        
        var detectResponse = await _faceClient.DetectAsync(photoData, FaceDetectionModel.Detection03, _faceRecognitionModel, false, [FaceAttributeType.QualityForRecognition]);
        
        var facesInImage = detectResponse.Value;
        
        foreach (FaceDetectionResult face in facesInImage)
        {
            QualityForRecognition? faceQualityForRecognition = face.FaceAttributes.QualityForRecognition;
            
            //  Only "high" quality images are recommended for person enrollment
            if (faceQualityForRecognition.HasValue && (faceQualityForRecognition.Value == QualityForRecognition.High))
            {
                await _largePersonGroupClient.AddFaceAsync(personId, photoData, detectionModel: FaceDetectionModel.Detection03);
            }
            else
            {
                Console.WriteLine("quality is too low");
            }
        }

        
        //Training will take longer time with more faces
        //Right now it's trained in less than 10ms with 2-5 faces
        var operation = await _largePersonGroupClient.TrainAsync(WaitUntil.Completed);

        await operation.WaitForCompletionResponseAsync();
        
        //pausing to not trigger rate limit, 60 secs
        await Task.Delay(60000);
        
    }


    private async Task CreatePersonGroupIfNotExistsAsync()
    {
        try
        {
            await _largePersonGroupClient.GetLargePersonGroupAsync();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var group = await _largePersonGroupClient.CreateAsync(_faceRecognizerSettings.PersonGroupId);
        }
    }
}