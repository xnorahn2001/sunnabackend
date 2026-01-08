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
    // [Authorize(Roles = "Admin")] // Uncomment to enforce Admin only
    public class AdminContentController : ControllerBase
    {
        private readonly SonnaDbContext _context;
        private readonly IFileService _fileService;

        public AdminContentController(SonnaDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpGet("get-dashboard-analytics")]
        public async Task<IActionResult> GetDashboardAnalytics()
        {
            var stats = new
            {
                UsersCount = await _context.Users.CountAsync(),
                ProjectsCount = await _context.Projects.CountAsync(),
                NewsCount = await _context.News.CountAsync(),
                ProductsCount = await _context.Products.CountAsync(),
                CampsCount = await _context.Camps.CountAsync(),
                PodcastsCount = await _context.Podcasts.CountAsync(),
                ExpertsCount = await _context.Experts.CountAsync()
            };
            return Ok(stats);
        }

        // --- Data Retrieval Endpoints ---

        [HttpGet("get-all-news")]
        public async Task<IActionResult> GetAllNews() => Ok(await _context.News.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts() => Ok(await _context.Products.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-all-camps")]
        public async Task<IActionResult> GetAllCamps() => Ok(await _context.Camps.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-all-podcasts")]
        public async Task<IActionResult> GetAllPodcasts() => Ok(await _context.Podcasts.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-all-experts")]
        public async Task<IActionResult> GetAllExperts() => Ok(await _context.Experts.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers() => Ok(await _context.Users.OrderByDescending(x => x.CreatedAt).ToListAsync());

        [HttpGet("get-applications")]
        public async Task<IActionResult> GetApplications() 
        {
            // Assuming applications refer to Projects uploaded by users
            var projects = await _context.Projects.Include(p => p.User).OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(projects);
        }

        [HttpGet("get-partners")]
        public IActionResult GetPartners() 
        {
            // Placeholder: If you have a Partners table, fetch from it. 
            // Returning empty list for now to satisfy the frontend call.
            return Ok(new List<object>()); 
        }

        [HttpGet("get-settings")]
        public async Task<IActionResult> GetSettings() => Ok(await _context.SystemSettings.ToListAsync());

        // --- Content Management ---

        [HttpPost("{type}")]
        public async Task<IActionResult> AddContent(string type, [FromForm] ContentCreateDto dto)
        {
            string imageUrl = "";
            if (dto.Image != null)
            {
                imageUrl = await _fileService.UploadFileAsync(dto.Image);
            }

            switch (type.ToLower())
            {
                case "news":
                    _context.News.Add(new News { Title = dto.Title, Description = dto.Description, ImageUrl = imageUrl });
                    break;
                case "products":
                case "product":
                    _context.Products.Add(new Product { Title = dto.Title, Description = dto.Description, ImageUrl = imageUrl });
                    break;
                case "camps":
                case "camp":
                    _context.Camps.Add(new Camp { Title = dto.Title, Description = dto.Description, ImageUrl = imageUrl });
                    break;
                case "podcasts":
                case "podcast":
                    _context.Podcasts.Add(new Podcast { Title = dto.Title, Description = dto.Description, ImageUrl = imageUrl });
                    break;
                case "experts":
                case "expert":
                    // Mapping Title to Name for Expert
                    _context.Experts.Add(new Expert { Name = dto.Title, Description = dto.Description, ImageUrl = imageUrl });
                    break;
                default:
                    return BadRequest("Invalid content type");
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Content added successfully" });
        }

        [HttpDelete("delete/{type}/{id}")]
        public async Task<IActionResult> DeleteContent(string type, int id)
        {
            object? entity = null;
            switch (type.ToLower())
            {
                case "news":
                    entity = await _context.News.FindAsync(id);
                    if (entity != null) _context.News.Remove((News)entity);
                    break;
                case "products":
                case "product":
                    entity = await _context.Products.FindAsync(id);
                    if (entity != null) _context.Products.Remove((Product)entity);
                    break;
                case "camps":
                case "camp":
                    entity = await _context.Camps.FindAsync(id);
                    if (entity != null) _context.Camps.Remove((Camp)entity);
                    break;
                case "podcasts":
                case "podcast":
                    entity = await _context.Podcasts.FindAsync(id);
                    if (entity != null) _context.Podcasts.Remove((Podcast)entity);
                    break;
                case "experts":
                case "expert":
                    entity = await _context.Experts.FindAsync(id);
                    if (entity != null) _context.Experts.Remove((Expert)entity);
                    break;
                default:
                    return BadRequest("Invalid content type");
            }

            if (entity == null) return NotFound();

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Deleted successfully" });
        }

        [HttpPost("update-config")]
        public async Task<IActionResult> UpdateConfig([FromBody] SystemSettingDto dto)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == dto.Key);
            if (setting == null)
            {
                setting = new SystemSetting { Key = dto.Key, Value = dto.Value };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = dto.Value;
            }
            
            await _context.SaveChangesAsync();
            return Ok(setting);
        }
    }

    public class ContentCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }

    public class SystemSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
