using System;
using System.ComponentModel.DataAnnotations.Schema;
using Google.Protobuf.WellKnownTypes;

public class Comment
{
    public int CommentId { get; set; }
    public required string AuthorId { get; set; }
    public virtual required ApplicationUser Author { get; set; }
    public required string Content { get; set; }
    public DateTime DatePosted { get; set; }
    public int BlogPostId { get; set; }
    public virtual required BlogPost BlogPost { get; set; }
}
