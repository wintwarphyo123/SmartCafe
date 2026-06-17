using Microsoft.AspNetCore.Identity;

namespace SmartCafe.Interfaces
{
    public interface IJwtService
    {
        Task<String> GenerateToke(IdentityUser user);
    }
}
