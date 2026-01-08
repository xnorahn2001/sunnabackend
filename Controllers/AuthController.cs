using Microsoft.AspNetCore.Mvc;
using SonnaBackend.Controllers.DTOs;
using SonnaBackend.Models;
using SonnaBackend.Services;

namespace SonnaBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;

        public AuthController(IAuthService authService, INotificationService notificationService)
        {
            _authService = authService;
            _notificationService = notificationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserType = model.UserType,
                CRNumber = model.CRNumber
            };

            try
            {
                var createdUser = await _authService.RegisterAsync(user, model.Password);
                
                // Notify Admin
                await _notificationService.SendNotificationAsync("New User Registered", $"User {user.FullName} ({user.UserType}) has joined.");

                // Auto-Login: Generate token immediately
                // We use the same password provided in registration model
                // Note: Identify by PhoneNumber (default) or CRNumber if PhoneNumber is empty/null, though simplified here to try both or rely on what's provided.
                // Assuming PhoneOrCR logic in LoginAsync handles either. We pass PhoneNumber as primary identifier here.
                var identifier = !string.IsNullOrEmpty(model.PhoneNumber) ? model.PhoneNumber : model.CRNumber;
                var token = await _authService.LoginAsync(identifier, model.Password);

                return Ok(new { User = createdUser, Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto model)
        {
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserType = "Admin",
                CRNumber = model.CRNumber
            };

            try
            {
                var createdUser = await _authService.RegisterAsync(user, model.Password);
                
                // Notify (Optional)
                await _notificationService.SendNotificationAsync("New Admin Registered", $"Admin {user.FullName} has joined.");

                // Auto-Login: Generate token immediately
                var identifier = !string.IsNullOrEmpty(model.PhoneNumber) ? model.PhoneNumber : model.CRNumber;
                var token = await _authService.LoginAsync(identifier, model.Password);

                return Ok(new { User = createdUser, Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var token = await _authService.LoginAsync(model.PhoneOrCR, model.Password);
            if (token == null) return Unauthorized("Invalid credentials");

            return Ok(new { Token = token });
        }
    }
}
