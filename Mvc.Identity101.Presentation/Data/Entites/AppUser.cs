using Microsoft.AspNetCore.Identity;

namespace Mvc.Identity101.Data.Entites;

public class AppUser : IdentityUser
{
    public string? City { get; set; }
}