using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Marcet_DB
{
    public class Program2
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Получаем строку подключения из файла конфигурации
            string connection = builder.Configuration.GetConnectionString("DefaultConnection");

            // Добавляем контекст ApplicationContext в качестве сервиса в приложение
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

            var app = builder.Build();

            app.MapGet("/", (ApplicationContext db) =>
            {
                return db.Customers.ToList();
            });

            // Добавьте здесь код для настройки маршрутов и обработки запросов

            app.Run();
        }
    }
}
