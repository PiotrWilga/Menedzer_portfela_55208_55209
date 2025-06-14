using System.Security.Claims;

namespace PersonalFinanceManager.WebApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst("id");
            return idClaim != null && int.TryParse(idClaim.Value, out var id) ? id : (int?)null;
        }

        public static string? GetLogin(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
