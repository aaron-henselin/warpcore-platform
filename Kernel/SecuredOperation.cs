using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WarpCore.Platform.Kernel
{
    public enum PermissionType
    {
        Grant,
        Deny
    }

    public class KnownPrivilegeNames
    {
        public const string Create = nameof(Create);
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
    }

    public class PermissionRuleSet : List<IPermissionRule>
    {
        public PermissionRuleSet()
        {
        }

        public PermissionRuleSet(IEnumerable<IPermissionRule> defaultPermissions)
        {
            this.AddRange(defaultPermissions);
        }
    }

    public interface IPermissionRule
    {
        Guid SecuredResourceId { get; }
        string AppliesToRoleName { get; }

        PermissionType PermissionType { get; }

        string PrivilegeName { get; }
    }

    public class PermissionSetEvaluator
    {
        public static void Assert(PermissionRuleSet rules, string privilegeName)
        {
            var myRoles = new RoleProvider().GetRoles();

            foreach (var rule in rules)
            {
                if (rule.PermissionType != PermissionType.Deny)
                    continue;

                if (rule.PrivilegeName != privilegeName)
                    continue;

                if (myRoles.Contains(rule.AppliesToRoleName))
                    throw new SecurityException("Denied via Deny: "+rule.AppliesToRoleName);
            }

            var grantRulesAvailable = rules.Where(x => x.PermissionType == PermissionType.Grant && x.PrivilegeName == privilegeName).ToList();
            var grantRulesMet = grantRulesAvailable.Where(x => myRoles.Contains(x.AppliesToRoleName));

            if (grantRulesAvailable.Any() && !grantRulesMet.Any())
               throw new SecurityException("Denied via Grant. Needed one of the following roles: " + string.Join(", ", grantRulesAvailable));
            
        }
    }

    public class RoleProvider
    {
        public string[] GetRoles()
        {
            return new[] {"Everyone","BackendUsers" };
        }
    }



}
