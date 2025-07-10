using Microsoft.AspNetCore.Identity;

namespace Taskit.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}