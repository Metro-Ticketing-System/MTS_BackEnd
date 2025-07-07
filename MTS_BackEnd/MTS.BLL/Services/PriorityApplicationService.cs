using Microsoft.AspNetCore.Http;
using MTS.DAL.Dtos;
using MTS.DAL.Repositories;
using MTS.Data.Enums;
using MTS.Data.Models;

namespace MTS.BLL.Services
{
	public interface IPriorityApplicationService
	{
		public Task<bool> CreateAsync(Guid passengerId, PriorityType type, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImag);
		public Task<List<PriorityApplicationDto>> GetAllPriorityApplicationsAsync();
		public Task<bool> UpdatePriorityApplicationStatusAsync(int applicationId, ApplicationStatus status, string? note, Guid adminId);
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

		public async Task<bool> CreateAsync(Guid passengerId, PriorityType type, IFormFile frontIdCardImage, IFormFile backIdCardImage, IFormFile? studentCardImage, IFormFile? revolutionaryContributorImage)
		{
			try
			{
				var user = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == passengerId && u.IsActive == true);
				if (user == null)
				{
					Console.WriteLine("User not found or inactive!");
					return false;
				}

				var existingApplication = await _unitOfWork.GetRepository<PriorityApplication>().GetByPropertyAsync(a => a.PassengerId == passengerId &&
				(a.Status == ApplicationStatus.Pending
				|| a.Status == ApplicationStatus.Approved));
				if (existingApplication != null)
				{
					Console.WriteLine("Passenger already has an approved/pending application and cannot submit another.");
					return false;
				}

				string frontIdCardImageUrl = string.Empty;
				string backIdCardImageUrl = string.Empty;
				string studentCardImageUrl = string.Empty;
				string revolutionaryContributorImageUrl = string.Empty;
				if (frontIdCardImage != null && frontIdCardImage.Length > 0)
				{
					frontIdCardImageUrl = await _supabaseFileService.UploadFileAsync(frontIdCardImage, "priority-documents", passengerId.ToString());
				}

				if (backIdCardImage != null && backIdCardImage.Length > 0)
				{
					backIdCardImageUrl = await _supabaseFileService.UploadFileAsync(backIdCardImage, "priority-documents", passengerId.ToString());
				}

				if (studentCardImage != null && studentCardImage.Length > 0)
				{
					studentCardImageUrl = await _supabaseFileService.UploadFileAsync(studentCardImage, "priority-documents", passengerId.ToString());
				}

				if (revolutionaryContributorImage != null && revolutionaryContributorImage.Length > 0)
				{
					revolutionaryContributorImageUrl = await _supabaseFileService.UploadFileAsync(revolutionaryContributorImage, "priority-documents", passengerId.ToString());
				}

				var priorityApplication = new PriorityApplication
				{
					PassengerId = passengerId,
					CreatedBy = user.UserName,
					CreatedTime = DateTime.Now,
					Type = type,
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

		public async Task<List<PriorityApplicationDto>> GetAllPriorityApplicationsAsync()
		{
			try
			{
				var applications = await _unitOfWork.GetRepository<PriorityApplication>().GetAllByPropertyAsync(includeProperties: "Passenger, Admin");
				if (applications == null || applications.Count == 0)
				{
					Console.WriteLine("No priority applications found.");
					return new List<PriorityApplicationDto>();
				}
				var applicationDtos = applications.Select(app => new PriorityApplicationDto
				{
					Id = app.Id,
					CreatedBy = app.CreatedBy,
					CreatedTime = app.CreatedTime,
					Type = app.Type,
					FrontIdCardImageUrl = app.FrontIdCardImageUrl,
					BackIdCardImageUrl = app.BackIdCardImageUrl,
					StudentCardImageUrl = app.StudentCardImageUrl,
					RevolutionaryContributorImageUrl = app.RevolutionaryContributorImageUrl,
					Status = app.Status,
					PassengerName = app.Passenger.LastName + " " + app.Passenger.FirstName,
					AdminName = app.Admin?.LastName + " " + app.Admin?.FirstName,
					UpdatedBy = app.UpdatedBy,
					LastUpdatedTime = app.LastUpdatedTime,
					Note = app.Note

				}).ToList();

				return applicationDtos;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving priority applications: {ex.Message}");
				return new List<PriorityApplicationDto>();
			}
		}

		public async Task<bool> UpdatePriorityApplicationStatusAsync(int applicationId, ApplicationStatus status, string? note, Guid adminId)
		{
			try
			{
				var application = await _unitOfWork.GetRepository<PriorityApplication>().GetByPropertyAsync(x => x.Id == applicationId);
				if (application == null)
				{
					Console.WriteLine("Priority application not found.");
					return false;
				}

				var admin = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == adminId && u.IsActive == true);
				if (admin == null)
				{
					Console.WriteLine("Admin not found or inactive.");
					return false;
				}

				application.Status = status;
				application.Note = note;
				application.AdminId = adminId;
				application.UpdatedBy = admin.UserName;
				application.LastUpdatedTime = DateTime.Now;

				await _unitOfWork.GetRepository<PriorityApplication>().UpdateAsync(application);

				var user = await _unitOfWork.GetRepository<User>().GetByPropertyAsync(u => u.Id == application.PassengerId && u.IsActive == true);

				if (application.Status == ApplicationStatus.Approved)
				{
					if (application.Type == PriorityType.Student)
					{
						user!.IsStudent = true;
						user!.IsRevolutionaryContributor = false;
						await _unitOfWork.GetRepository<User>().UpdateAsync(user!);
					}
					else
					{
						user!.IsStudent = false;
						user!.IsRevolutionaryContributor = true;
						await _unitOfWork.GetRepository<User>().UpdateAsync(user!);
					}
				}

				var result = await _unitOfWork.SaveAsync();
				if (result > 0)
				{
					Console.WriteLine("Priority application status updated successfully!");
					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error updating priority application status: {ex.Message}");
				return false;
			}
		}
	}
}
