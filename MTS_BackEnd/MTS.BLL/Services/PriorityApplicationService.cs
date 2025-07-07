using Microsoft.AspNetCore.Http;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IPriorityApplicationService
	{
		public Task<bool> CreateAsync(CreatePriorityApplicationDto dto, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImag);
	}
	public class PriorityApplicationService : IPriorityApplicationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ISupabaseFileService _supabaseFileService;
		public PriorityApplicationService()
		{
		}

		public PriorityApplicationService(IUnitOfWork unitOfWork, ISupabaseFileService supabaseFileService)
		{
			_unitOfWork = unitOfWork;
			_supabaseFileService = supabaseFileService;
		}

		public async Task<bool> CreateAsync(CreatePriorityApplicationDto dto, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImage)
		{
			try
			{
				var user = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == dto.PassengerId && u.IsActive == true);
				if (user == null)
				{
					Console.WriteLine("User not found or inactive!");
					return false;
				}

				var application = await _unitOfWork.GetRepository<PriorityApplication>().GetByPropertyAsync(a => a.PassengerId == dto.PassengerId);
				if (application != null)
				{
					Console.WriteLine("Priority application already exists for this passenger!");
					return false;
				}

				string frontIdCardImageUrl = string.Empty;
				string backIdCardImageUrl = string.Empty;
				string studentCardImageUrl = string.Empty;
				string revolutionaryContributorImageUrl = string.Empty;
				if (frontIdCardImage != null && frontIdCardImage.Length > 0)
				{
					frontIdCardImageUrl = await _supabaseFileService.UploadFileAsync(frontIdCardImage, "priority-documents", dto.PassengerId.ToString());
				}

				if (backIdCardImage != null && backIdCardImage.Length > 0)
				{
					backIdCardImageUrl = await _supabaseFileService.UploadFileAsync(backIdCardImage, "priority-documents", dto.PassengerId.ToString());
				}

				if (studentCardImage != null && studentCardImage.Length > 0)
				{
					studentCardImageUrl = await _supabaseFileService.UploadFileAsync(studentCardImage, "priority-documents", dto.PassengerId.ToString());
				}

				if (revolutionaryContributorImage != null && revolutionaryContributorImage.Length > 0)
				{
					revolutionaryContributorImageUrl = await _supabaseFileService.UploadFileAsync(revolutionaryContributorImage, "priority-documents", dto.PassengerId.ToString());
				}

				var priorityApplication = new PriorityApplication
				{
					PassengerId = dto.PassengerId,
					CreatedBy = user.UserName,
					CreatedTime = DateTime.Now,
					Type = dto.Type,
					FrontIdCardImageUrl = frontIdCardImageUrl,
					BackIdCardImageUrl = backIdCardImageUrl,
					StudentCardImageUrl = studentCardImageUrl,
					RevolutionaryContributorImageUrl = revolutionaryContributorImageUrl,
				};

				await _unitOfWork.GetRepository<PriorityApplication>().AddAsync(priorityApplication);
				var result = await _unitOfWork.SaveAsync();
				if (result > 0)
				{
					Console.WriteLine("Priority application created successfully!");
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating priority application: {ex.Message}");
				return false;
			}
		}
	}
}
