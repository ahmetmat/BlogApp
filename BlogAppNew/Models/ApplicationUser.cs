using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    //You can add additional properties here
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? bio { get; set; }

}
