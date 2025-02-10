using Api.Model;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PhotoController : ControllerBase
	{
		private readonly FaceRecognitionService _faceService;
		public PhotoController(FaceRecognitionService faceService)
		{
			_faceService = faceService;
		}
		[HttpPost("compare")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> CompareFaces(IFormFile image1, [FromForm] IFormFile image2)
		{

			if (image1 == null || image2 == null)
				return BadRequest("Please provide both images.");

			using var stream1 = image1.OpenReadStream();
			using var stream2 = image2.OpenReadStream();

			var isMatch = await _faceService.CompareFacesAsync(stream1, stream2);

			return Ok(new { STATUS = isMatch });
		}
		[HttpPost("CheckValidPhoto")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> CheckValidPhoto(IFormFile? image1)
		{
			PhotoResponce response = new PhotoResponce
			{
				STATUS = false,
				MESSAGE = "Image could not be blanck!"
			};
			if (image1 == null)
			{
				return Ok(response);
			}
			using var stream1 = image1.OpenReadStream();


			 response = await _faceService.CheckFacesAsync(stream1);

			return Ok(response);
		}
	}
}
