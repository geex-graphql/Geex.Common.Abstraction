using Geex.Common.Abstractions;

namespace Geex.Common.Authorization.Abstraction
{
    public class AppPermission : Enumeration<AppPermission, string>
    {
        public AppPermission(string value) : base(value)
        {
        }

        public static AppPermission AssignRole { get; } = new AppPermission(nameof(AssignRole));
    }
}