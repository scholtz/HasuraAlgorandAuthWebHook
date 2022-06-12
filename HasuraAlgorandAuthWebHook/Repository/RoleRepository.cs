using System.Collections.Concurrent;

namespace HasuraAlgorandAuthWebHook.Repository
{
    public class RoleRepository
    {
        private ConcurrentDictionary<string, string> Roles = new ConcurrentDictionary<string, string>();
        public RoleRepository( IConfiguration configuration)
        {
          
            var roles = configuration.GetSection("roles")?.Get<List<Model.AccountRole>>();
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!string.IsNullOrEmpty(role.Address))
                    {
                        Roles[role.Address] = role.Role;
                    }
                }
            }

        }

        internal string GetRole(string account)
        {
            if(Roles.TryGetValue(account, out var role))
            {
                return role;
            }
            return "User";
        }

        internal string Stats()
        {
            return $"{Roles.Count} roles configured";
        }
    }
}
