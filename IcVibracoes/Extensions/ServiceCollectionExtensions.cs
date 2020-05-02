using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace IcVibracoes.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// This method configures the documentation file for Swagger User Interface.
        /// </summary>
        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            //string assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(assemblyTitle, new OpenApiInfo
                {
                    Title = assemblyTitle,
                    //Description = assemblyDescription,
                    Version = "v1"
                });
            });

            return services;
        }
    }
}
