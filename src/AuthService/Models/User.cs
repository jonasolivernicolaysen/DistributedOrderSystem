using Microsoft.AspNetCore.Identity;

namespace AuthService.Models;

public class User : IdentityUser
{
    public decimal AccountBalance { get; set; } = 1000;
}