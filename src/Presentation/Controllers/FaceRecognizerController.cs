using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaceRecognizerController : ControllerBase
{
    private readonly IFaceRecognizerService _faceRecognizerService;

    public FaceRecognizerController(IFaceRecognizerService faceRecognizerService)
    {
        _faceRecognizerService = faceRecognizerService;
    }
    
    [HttpPost]
    [Route("addFace")]
    public async Task<IActionResult> AddFace([FromForm] string name, [FromForm] IFormFile image)
    {
        var result = await BinaryData.FromStreamAsync(image.OpenReadStream());
        await _faceRecognizerService.AddPersonWithFaceAsync(name, result);
        return Ok();
    }
}