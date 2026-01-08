using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SonnaBackend.Data;
using SonnaBackend.Models;
using SonnaBackend.Services;

namespace SonnaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires Login
    public class UserController : ControllerBase
    {
        private readonly SonnaDbContext _context;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;

        public UserController(SonnaDbContext context, IFileService fileService, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) throw new UnauthorizedAccessException();
            return int.Parse(idClaim.Value);
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            
            // Should email be updatable? usually requires verification. Assuming yes for simpler scope.
            // user.Email = dto.Email; 
            
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("upload-project")]
        public async Task<IActionResult> UploadProject([FromForm] UploadProjectDto dto)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            string fileUrl = "";
            if (dto.File != null)
            {
                fileUrl = await _fileService.UploadFileAsync(dto.File);
            }

            var project = new Project
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                FileUrl = fileUrl,
                Status = "Pending"
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Notify Admin
            await _notificationService.SendNotificationAsync("New Project Uploaded", 
                $"User {user.FullName} ({user.UserType}) uploaded a new project: {project.Title}.");

            return Ok(project);
        }

        [HttpGet("my-projects")]
        public async Task<IActionResult> GetMyProjects()
        {
            var userId = GetUserId();
            var projects = await _context.Projects
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(projects);
        }
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UploadProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}
