using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MTS.BLL;
using MTS.Data.Enums;
using System.ComponentModel.DataAnnotations;
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
		[HttpPost("create")]
		public async Task<IActionResult> Create([FromForm] PriorityType type, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImage)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(userId)) return Unauthorized("User not authenticated!");
			var userIdGuid = Guid.Parse(userId);

			if (type == PriorityType.Student && studentCardImage == null)
			{
				return BadRequest("Student card image is required for student priority type.");
			}

			if (type == PriorityType.RevolutionaryContributor && revolutionaryContributorImage == null)
			{
				return BadRequest("Revolutionary contributor image is required for revolutionary contributor priority type.");
			}

			var result = await _serviceProviders.PriorityApplicationService.CreateAsync(userIdGuid, type, frontIdCardImage, backIdCardImage, studentCardImage, revolutionaryContributorImage);
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
		[HttpGet("get-all")]
		public async Task<IActionResult> GetAll()
		{
			var applications = await _serviceProviders.PriorityApplicationService.GetAllPriorityApplicationsAsync();
			if (applications == null || !applications.Any())
			{
				return NotFound("No priority applications found.");
			}
			return Ok(applications);
		}


		[Authorize(Roles = "1")]
		[HttpPatch("set-status")]
		public async Task<IActionResult> SetStatus([FromQuery][Required] int applicationId, [Required]ApplicationStatus applicationStatus, string? note)
		{
			var adminId = User.FindFirstValue("id");
			if (string.IsNullOrEmpty(adminId)) return Unauthorized("User not authenticated!");
			var adminIdGuid = Guid.Parse(adminId);
			var isSucceed = await _serviceProviders.PriorityApplicationService.UpdatePriorityApplicationStatusAsync(applicationId, applicationStatus, note, adminIdGuid);
			if (!isSucceed)
			{
				return NotFound("No priority applications found.");
			}
			return NoContent();
		}

		[Authorize(Roles = "1")]
		[HttpGet("detail/{applicationId}")]
		public async Task<IActionResult> Get([FromRoute][Required] int applicationId)
		{
			var application = await _serviceProviders.PriorityApplicationService.GetPriorityApplicationAsync(applicationId);
			if (application == null)
			{
				return NotFound("No priority applications found.");
			}
			return Ok(application);
		}
	}
}
