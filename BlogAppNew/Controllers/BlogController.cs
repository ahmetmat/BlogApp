using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/[controller]")]
[ApiController]

public class BlogController : ControllerBase
{
    private readonly BlogAppContext _context;
    public int LikedCount { get; set; } = 0;


    public BlogController(BlogAppContext context)
    {
        _context = context;
    }
    // Is it All posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetPosts()
    {
        var blogPosts = await _context.BlogPosts
            .Include(bp => bp.Author)
            .ToListAsync();

        return Ok(blogPosts);
    }
    //Is it spesific posts
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetPost(int id)
    {
        var blogPost = await _context.BlogPosts
            .Include(bp => bp.Author)
            .FirstOrDefaultAsync(bp => bp.BlogPostId == id);

        if (blogPost == null)
        {
            return NotFound();
        }

        return Ok(blogPost);
    }
    [HttpPost]
    public async Task<IActionResult> PostDocument([FromBody] BlogPost blogPost)
    {
        //ModelState.Remove("Author");
        //ModelState.Remove("Comments");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // blogpost it save to databese
        var random = new Random();
        int randomId = random.Next(1, 1000000); // 1 ile 1,000,000 arasında bir değer üretir

        // Assign the generated random number to BlogPostId
        blogPost.BlogPostId = randomId;
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        // Successfully organized daily response turnaround
        return CreatedAtAction(nameof(GetPost), new { id = blogPost.BlogPostId }, blogPost);
    }
    [HttpGet("ByEmail/{email}")]
    public async Task<IActionResult> GetBlogPostsByEmail(string email)
    {
        var blogPosts = await _context.BlogPosts
            .Include(bp => bp.Author)
            .Where(bp => bp.Author.Email == email)
            .ToListAsync();

        if (!blogPosts.Any())
        {
            return NotFound("Bu yazarın makaleleri bulunamadı.");
        }

        return Ok(blogPosts);
    }
    // follow to category
    [HttpPost("follow")]
    public async Task<IActionResult> FollowCategory([FromBody] FollowCategoryRequest request)
    {
        var userCategory = await _context.UserCategories
            .FirstOrDefaultAsync(uc => uc.UserId == request.UserId && uc.CategoryId == request.CategoryId);

        if (userCategory != null)
        {
            return BadRequest("Bu kategori zaten takip ediliyor.");
        }

        var newUserCategory = new UserCategory
        {
            UserId = request.UserId,
            CategoryId = request.CategoryId
        };

        _context.UserCategories.Add(newUserCategory);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kategori başarıyla takip edildi." });
    }

}

public class FollowCategoryRequest
{
    public string UserId { get; set; }
    public int CategoryId { get; set; }
}



