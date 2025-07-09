using Microsoft.AspNetCore.Identity;

namespace Taskit.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}