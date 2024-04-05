public class Category
{
    public int CategoryId { get; set; }
    public required string Name { get; set; }
    public virtual required ICollection<BlogPost> BlogPosts { get; set; }
}
