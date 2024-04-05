using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BlogAppContext : IdentityDbContext<ApplicationUser>
{


    public BlogAppContext(DbContextOptions<BlogAppContext> options)
        : base(options)
    {
    }
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public DbSet<UserCategory> UserCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comment>().Ignore(c => c.DatePosted); // Ignore DatePosted in EF model
    }


}
