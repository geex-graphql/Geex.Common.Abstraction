namespace Geex.Common.Abstractions.Enumerations
{
    public class GeexClaimType : Enumeration<GeexClaimType, string>
    {
        public const string _Sub = "sub";
        public static GeexClaimType Sub { get; } = new GeexClaimType(nameof(Sub), _Sub);
        public const string _Nickname = "nick_name";
        public static GeexClaimType Nickname { get; } = new GeexClaimType(nameof(Nickname), _Nickname);
        public const string _Provider = "login_provider";
        public static GeexClaimType Provider { get; } = new GeexClaimType(nameof(Provider), _Provider);
        public const string _Expires = "expires";
        public static GeexClaimType Expires { get; } = new GeexClaimType(nameof(Expires), _Expires);
        public const string _Avatar = "avatar";
        public static GeexClaimType Avatar { get; } = new GeexClaimType(nameof(Avatar), _Avatar);
        public GeexClaimType(string name, string value) : base(name, value)
        {
        }
        
    }
}