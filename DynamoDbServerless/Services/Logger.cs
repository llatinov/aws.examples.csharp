using Serilog;
using Serilog.Formatting.Compact;

namespace DynamoDbServerless.Services
{
    public class Logger : ILogger
    {
        private static Serilog.Core.Logger _logger;

        public Logger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();
        }

        public void LogInformation(string messageTemplate, params object[] arguments)
        {
            _logger.Information(messageTemplate, arguments);
        }
    }
}