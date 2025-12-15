using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;

namespace RusticaPortal_PRMVAN.Api.Helpers
{


    public class LogHelper<T>
    {
        private readonly ILogger<T> _logger;

        public LogHelper(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Info(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void Error(string message, Exception ex = null, params object[] args)
        {
            if (ex != null)
                _logger.LogError(ex, message, args);
            else
                _logger.LogError(message, args);
        }

        public void Critical(string message, Exception ex = null, params object[] args)
        {
            if (ex != null)
                _logger.LogCritical(ex, message, args);
            else
                _logger.LogCritical(message, args);
        }
    }
}
