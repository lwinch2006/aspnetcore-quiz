using Microsoft.Extensions.DependencyInjection;
using QuizService.Common.Logic;
using QuizService.Common.Repositories;

namespace QuizService.Services.ServiceCollection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDatabaseClasses(this IServiceCollection services, string databaseConnectionString)
        {
            // Repositories.
            services.AddScoped<QuizRepository>();

            // Logic.
            services.AddScoped<QuizLogic>();
        }        
    }
}