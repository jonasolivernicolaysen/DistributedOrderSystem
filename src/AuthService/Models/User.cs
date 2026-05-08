using Microsoft.AspNetCore.Identity;

namespace AuthService.Models;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}