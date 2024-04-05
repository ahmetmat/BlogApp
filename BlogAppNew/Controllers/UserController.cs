using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly BlogAppContext _context;

    public UserController(BlogAppContext context)
    {
        _context = context;
    }

    [HttpPost("UserDetails")]
    public async Task<ActionResult> GetUserDetails([FromBody] UserRequest request)
    {
        var user = await _context.Users
            .Where(u => u.Email == request.Email)
            .Select(u => new { u.Name, u.Surname ,u.bio,u.Email,u.Id})
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "Kullanıcı bulunamadı." });
        }

        return Ok(user);
    }
    [HttpGet("getByEmail")]
    public IActionResult GetUserByEmail(string email)
    {
        // Find user by email address
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user != null)
        {
            // If the user is found, return with its ID
            return Ok(new { UserId = user.Id });
        }
        else
        {
            // Kullanıcı bulunamadıysa, uygun bir hata mesajı dön
            return NotFound("Kullanıcı bulunamadı.");
        }
    }
}


public class UserRequest
{
    public string Email { get; set; }
}
