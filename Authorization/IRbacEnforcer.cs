using System.Collections.Generic;
using System.Threading.Tasks;

using NetCasbin.Abstractions;

namespace Geex.Common.Abstraction.Authorization
{
    public interface IRbacEnforcer
    {
        List<string> GetRolesForUser(string name, string domain = null);
        List<string> GetUsersForRole(string name, string domain = null);
        public bool AddResourceGroupPolicy(string resourceId, string groupId);

        public bool AddUserGroupPolicy(string sub, string sub_group);

        public bool DeleteResourceGroupPolicy(string resourceOrGroupName);

        public bool Enforce(string sub, string mod, string act, string obj, string fields = "");

        public Task<bool> EnforceAsync(string sub, string mod, string act, string obj, string fields = "");

        public List<PolicyItem> GetFeaturePolicies(string sub);

        public List<PolicyItem> GetResourcePolicy(string sub, string obj);


        public List<GroupPolicy> GetUserGroupPolicies(string sub);

        public bool SetFeaturePolicy(string sub, string[] objs);

        public Task SetPermissionsAsync(string subId, params string[] permissions);

        public bool SetResourcePolicy(string sub, string obj, string[] acts, string fields = "*");

        public Task SetRolesForUser(string userId, List<string> roles);

        public bool SetUserGroupPolicy(string sub, IEnumerable<string> sub_groups);
        public Task<bool> AddRolesForUserAsync(string user, IEnumerable<string> role, string domain = null);
    }
}