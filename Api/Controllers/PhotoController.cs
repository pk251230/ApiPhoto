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

		public PhotoController()
		{
			_faceService = new FaceRecognitionService();
		}

		[HttpPost("compare")]
		public async Task<IActionResult> CompareFaces([FromForm] IFormFile image1, [FromForm] IFormFile image2)
		{

			if (image1 == null || image2 == null)
				return BadRequest("Please provide both images.");

			using var stream1 = image1.OpenReadStream();
			using var stream2 = image2.OpenReadStream();

			var isMatch = await _faceService.CompareFacesAsync(stream1, stream2);

			return Ok(new { STATUS = isMatch });
		}
		[HttpPost("CheckValidPhoto")]
		public async Task<IActionResult> CheckValidPhoto([FromForm] IFormFile? image1)
		{
			if (image1 == null)
			{
				return Ok(new { STATUS = false });
			}
			using var stream1 = image1.OpenReadStream();


			var isMatch = await _faceService.CheckFacesAsync(stream1);

			return Ok(new { STATUS = isMatch });
		}
	}
}
