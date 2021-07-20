using HotChocolate;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace Geex.Common.Abstractions
{
    public class UserFriendlyErrorFilter : IErrorFilter
    {

        public UserFriendlyErrorFilter(ILoggerProvider? loggerProvider)
        {
            LoggerProvider = loggerProvider;

        }

        public ILoggerProvider LoggerProvider { get; }

        public IError OnError(IError error)
        {
            if (error.Exception is UserFriendlyException)
            {
                error.RemoveException();
            }

            if (error.Exception != default)
            {
                if (error.Exception?.TargetSite?.DeclaringType != default)
                {
                    LoggerProvider.CreateLogger(error.Exception.TargetSite.DeclaringType.FullName).LogException(error.Exception);
                }
                else
                {
                    LoggerProvider.CreateLogger("Null").LogException(error.Exception);
                }
            }
            return error;
        }
    }
}