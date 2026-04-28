using DevConnect.Data;
using DevConnect.DTOs;
using DevConnect.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DevConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly DevConnectDbContext _context;

        public UsersController(DevConnectDbContext context)
        {
            _context = context;
        }

        // GET: api/users/profile  - Get own profile
        [HttpGet("profile")]
        public async Task<ActionResult<User>> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/users/profile  - Update own profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDTO dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.Name = dto.Name;
            user.Age = dto.Age;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/users  - Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // DELETE: api/users/{id}  - Admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
