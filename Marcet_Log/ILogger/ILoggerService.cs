namespace Marcet_Log.ILogger
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogError(string message, Exception ex);
    }
}
