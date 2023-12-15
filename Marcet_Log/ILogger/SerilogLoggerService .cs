namespace Marcet_Log.ILogger
{
    public class SerilogLoggerService : ILoggerService
    {
        private readonly Serilog.ILogger logger;

        public SerilogLoggerService(Serilog.ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogInformation(string message)
        {
            logger.Information(message);
        }

        public void LogError(string message, Exception ex)
        {
            logger.Error(ex, message);
        }
    }
}

