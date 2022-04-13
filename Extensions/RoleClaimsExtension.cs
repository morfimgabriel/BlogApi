using Blog.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace Blog.Extensions
{
    public static class RoleClaimsExtension
    {
        public static IEnumerable<Claim> GetClaims(this User user)
        {
            var result = new List<Claim>()
            {
                new(ClaimTypes.Name, user.Email) // User.Identity.Email
            };

            result.AddRange(user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Slug)));

            return result;
        }
    }
}
