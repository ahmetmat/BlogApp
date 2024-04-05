public class UserCategory
{
    public int UserCategoryId { get; set; }
    public string UserId { get; set; } 
    public int CategoryId { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual Category Category { get; set; }
}
