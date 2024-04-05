using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Google.Protobuf.WellKnownTypes;

public class BlogPost
{
    public int BlogPostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required DateTime DatePublished { get; set; }
    public int LikedCount { get;set; }
    public float AverageReadingTime { get; set; }
    public string AuthorId { get; set; }
    public int CategoryId { get; set; }
    public virtual ApplicationUser Author { get; set; }
    public virtual ICollection<Comment> Comments { get; set; }
}
