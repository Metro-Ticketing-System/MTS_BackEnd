using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.DAL.Dtos;
using MTS.Data.Enums;
using System.Security.Claims;

namespace MTS.BackEnd.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PriorityApplicationController : Controller
	{
		private readonly IServiceProviders _serviceProviders;
		public PriorityApplicationController(IServiceProviders serviceProviders)
		{
			_serviceProviders = serviceProviders;
		}

		[Authorize(Roles = "3")]
		[HttpPost("Create")]
		public async Task<IActionResult> CreateAsync([FromForm] CreatePriorityApplicationDto dto, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImage)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (dto == null)
			{
				return BadRequest("Invalid data.");
			}

			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
			dto.PassengerId = Guid.Parse(userId);

			if (dto.Type == PriorityType.Student && studentCardImage == null)
			{
				return BadRequest("Student card image is required for student priority type.");
			}

			if (dto.Type == PriorityType.RevolutionaryContributor && revolutionaryContributorImage == null)
			{
				return BadRequest("Revolutionary contributor image is required for revolutionary contributor priority type.");
			}

			var result = await _serviceProviders.PriorityApplicationService.CreateAsync(dto, frontIdCardImage, backIdCardImage, studentCardImage, revolutionaryContributorImage);
			if (result)
			{
				return Ok("Priority application created successfully.");
			}
			else
			{
				return BadRequest("Failed to create priority application.");
			}
		}

		[Authorize(Roles = "1")]
		[HttpGet("GetAllPriorityApplication")]
		public async Task<IActionResult> GetAllAsync()
		{
			var applications = await _serviceProviders.PriorityApplicationService.GetAllPriorityApplicationsAsync();
			if (applications == null || !applications.Any())
			{
				return NotFound("No priority applications found.");
			}
			return Ok(applications);
		}
	}
}
