using Serilog;

namespace Marcet_DB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Generate a random key and print its Base64 representation
            GenerateAndPrintKey();

            // Continue with your existing code
            CreateHostBuilder(args).Build().Run();
        }

        private static void GenerateAndPrintKey()
        {
            var random = new Random();
            var keyBytes = new byte[16];
            random.NextBytes(keyBytes);

            var base64Key = Convert.ToBase64String(keyBytes);
            Console.WriteLine("Generated Key: " + base64Key);

            // Update your appsettings.json file with this generated key
            // Copy the printed key and replace the existing "Key" value in appsettings.json
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog(); // This line is added to use Serilog
    }
}
