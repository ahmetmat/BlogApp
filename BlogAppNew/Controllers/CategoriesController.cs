using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly BlogAppContext _context;

    public CategoriesController(BlogAppContext context)
    {
        _context = context;
    }

    // Endpoint that pulls blog posts and category names by category
    [HttpGet("{categoryId}/posts")]
    public async Task<ActionResult<IEnumerable<object>>> GetBlogPostsByCategory(int categoryId)
    {
        var category = await _context.Categories
            .Where(c => c.CategoryId == categoryId)
            .Include(c => c.BlogPosts)
            .Select(c => new
            {
                CategoryName = c.Name,
                Posts = c.BlogPosts.Select(bp => new
                {
                    bp.BlogPostId,
                    bp.Title,
                    bp.Content,
                    bp.DatePublished,
                    bp.Author,
                    bp.AuthorId,
                    bp.LikedCount
                })
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound("Category not found.");
        }

        return Ok(category);
    }
}
