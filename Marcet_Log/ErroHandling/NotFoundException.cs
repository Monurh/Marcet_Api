namespace Marcet_Log.ErrorHandling
{
    public class NotFoundException : Exception
    {
        public string ResourceName { get; }

        public NotFoundException(string resourceName, string message = "Resource not found") : base(message)
        {
            ResourceName = resourceName;
        }
    }

    public class UnauthorizedException : Exception
    {
        public string UserName { get; }

        public UnauthorizedException(string userName, string message = "Unauthorized access") : base(message)
        {
            UserName = userName;
        }
    }
}

