namespace JJMasterData.Web.Extensions;

using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal? user)
    {
        public bool IsInRole(params string[] roles)
        {
            if (user == null || roles.Length == 0)
                return false;

            foreach (var role in roles)
            {
                if (user.IsInRole(role))
                    return true;
            }

            return false;
        }
        
        public bool IsAdmin => user?.IsInRole("Admin") is true;
    }
}